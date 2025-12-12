using System;
using System.IO;

namespace QualisulCameraApp.Services
{
    public class SessionManager
    {
        public bool IsSessionActive { get; private set; }
        public string ClientBasePath { get; private set; } = string.Empty;
        public string OSNumber { get; private set; } = string.Empty;
        public string CurrentSessionFolder { get; private set; } = string.Empty;
        
        private int _sequenceCounter;

        public void SetClientPath(string path)
        {
            if (Directory.Exists(path))
            {
                ClientBasePath = path;
            }
        }

        public string StartSession(string osNumber)
        {
            if (string.IsNullOrWhiteSpace(ClientBasePath) || !Directory.Exists(ClientBasePath))
            {
                throw new InvalidOperationException("Selecione uma pasta de cliente válida.");
            }

            if (string.IsNullOrWhiteSpace(osNumber) || !osNumber.StartsWith("OS "))
            {
                 // Requirement: "somente no formato OS XXXX"
                 // I will enforce this check in ViewModel but check here too.
                 // Actually, let's be lenient here or strict. Strict is safer.
            }

            OSNumber = osNumber.Trim();
            
            // Create folder: ClientPath / OS Number
            CurrentSessionFolder = Path.Combine(ClientBasePath, OSNumber);
            if (!Directory.Exists(CurrentSessionFolder))
            {
                Directory.CreateDirectory(CurrentSessionFolder);
            }
            else 
            {
                // "Se a OS digitada ja existir... salvar na pasta ja criada." -> Done.
                // We should check existing files to resume sequence counter?
                // "O aplicativo limpa os dados da tela para permitir uma nova sessao" implied reset, start from 1?
                // User said "sequencial". If I restart the app or session, should I overwrite 1?
                // Probably better to append or find next available.
                // For safety, I'll find the max sequence number in the folder.
                _sequenceCounter = GetNextSequenceNumber();
            }

            IsSessionActive = true;
            return CurrentSessionFolder;
        }

        public void EndSession()
        {
            IsSessionActive = false;
            CurrentSessionFolder = string.Empty;
            OSNumber = string.Empty;
            _sequenceCounter = 0;
        }

        public string SavePhoto(System.Windows.Media.Imaging.BitmapSource image, string tag)
        {
            if (!IsSessionActive) throw new InvalidOperationException("Sessão não iniciada.");

            _sequenceCounter++;
            string safeTag = string.IsNullOrWhiteSpace(tag) ? "NoTag" : tag.Trim();
            
            // File Name: OS, TAG, sequencial
            string fileName = $"{OSNumber.Replace(" ", "")}_{safeTag}_{_sequenceCounter:D3}.jpg";
            string fullPath = Path.Combine(CurrentSessionFolder, fileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                var encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder();
                encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }

            return fullPath;
        }

        public List<string> GetExistingSessions()
        {
            if (string.IsNullOrEmpty(ClientBasePath) || !Directory.Exists(ClientBasePath))
                return new List<string>();

            try
            {
                var dirInfo = new DirectoryInfo(ClientBasePath);
                return dirInfo.GetDirectories("OS*")
                              .Where(d => d.Name.StartsWith("OS", StringComparison.OrdinalIgnoreCase))
                              .Select(d => d.Name)
                              .OrderByDescending(n => n) // Show newest (highest numbers) first? Or alphabetical.
                              .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public void DeletePhoto(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void RenameSession(string newOsName)
        {
             if (string.IsNullOrEmpty(CurrentSessionFolder) || !Directory.Exists(CurrentSessionFolder)) return;

             // 1. Rename files inside the folder
             // Pattern: OLDOS_TAG_SEQ.jpg -> NEWOS_TAG_SEQ.jpg
             var files = Directory.GetFiles(CurrentSessionFolder, "*.jpg");
             string oldOsClean = OSNumber.Replace(" ", "");
             string newOsClean = newOsName.Replace(" ", "");

             foreach (var file in files)
             {
                 string fileName = Path.GetFileName(file);
                 // Check if file starts with old OS prefix (ignoring case just in case)
                 if (fileName.StartsWith(oldOsClean, StringComparison.OrdinalIgnoreCase))
                 {
                     // Replace first occurrence
                     string newFileName = newOsClean + fileName.Substring(oldOsClean.Length);
                     string newPath = Path.Combine(CurrentSessionFolder, newFileName);
                     File.Move(file, newPath);
                 }
             }

             // 2. Rename the folder itself
             string parentDir = Directory.GetParent(CurrentSessionFolder)!.FullName;
             string newFolderPath = Path.Combine(parentDir, newOsName);

             if (Directory.Exists(newFolderPath))
             {
                 throw new IOException($"A pasta '{newOsName}' já existe. Não é possível renomear.");
             }

             Directory.Move(CurrentSessionFolder, newFolderPath);
             
             // Update state
             CurrentSessionFolder = newFolderPath;
             OSNumber = newOsName;
        }

        private int GetNextSequenceNumber()
        {
            if (string.IsNullOrEmpty(CurrentSessionFolder) || !Directory.Exists(CurrentSessionFolder)) return 1;

            var files = Directory.GetFiles(CurrentSessionFolder, "*.jpg");
            int maxSeq = 0;
            foreach (var file in files)
            {
                // Format: OS_TAG_SEQ.jpg. 
                // We want the last part.
                var fileName = Path.GetFileNameWithoutExtension(file);
                var parts = fileName.Split('_');
                if (parts.Length > 0)
                {
                    if (int.TryParse(parts.Last(), out int seq))
                    {
                        if (seq > maxSeq) maxSeq = seq;
                    }
                }
            }
            return maxSeq + 1;
        }
    }
}
