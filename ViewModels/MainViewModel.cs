using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QualisulCameraApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private ObservableCollection<string> _availableCameras = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableCameras
        {
            get => _availableCameras;
            set => SetProperty(ref _availableCameras, value);
        }

        private string? _selectedCamera;
        public string? SelectedCamera
        {
            get => _selectedCamera;
            set {
                if (SetProperty(ref _selectedCamera, value))
                {
                    OnSelectedCameraChanged(value);
                }
            }
        }

        private string _clientPath = "Nenhuma seleção...";
        public string ClientPath
        {
            get => _clientPath;
            set => SetProperty(ref _clientPath, value);
        }

        private string _osNumber = "OS XXXX";
        public string OSNumber
        {
            get => _osNumber;
            set => SetProperty(ref _osNumber, value);
        }

        private string _tagText = "";
        public string TagText
        {
            get => _tagText;
            set => SetProperty(ref _tagText, value);
        }

        private bool _isSessionActive = false;
        public bool IsSessionActive
        {
            get => _isSessionActive;
            set => SetProperty(ref _isSessionActive, value);
        }

        private ImageSource? _cameraFeed;
        public ImageSource? CameraFeed
        {
            get => _cameraFeed;
            set => SetProperty(ref _cameraFeed, value);
        }

        private ObservableCollection<string> _existingSessions = new ObservableCollection<string>();
        public ObservableCollection<string> ExistingSessions
        {
            get => _existingSessions;
            set => SetProperty(ref _existingSessions, value);
        }

        private string? _selectedExistingSession;
        public string? SelectedExistingSession
        {
            get => _selectedExistingSession;
            set {
                if(SetProperty(ref _selectedExistingSession, value))
                {
                    if(!string.IsNullOrEmpty(value))
                    {
                        OSNumber = value;
                    }
                }
            }
        }

        private ObservableCollection<Models.GalleryItem> _galleryPhotos = new ObservableCollection<Models.GalleryItem>();
        public ObservableCollection<Models.GalleryItem> GalleryPhotos => _galleryPhotos; 

        private readonly Services.CameraService _cameraService;
        private readonly Services.SessionManager _sessionManager;
        
        public MainViewModel()
        {
            _cameraService = new Services.CameraService();
            _sessionManager = new Services.SessionManager();

            _cameraService.FrameArrived += OnFrameArrived;
            
            // Initialize Cameras
            _ = RefreshCameras();
        }

        private void OnFrameArrived(BitmapSource frame)
        {
            CameraFeed = frame; 
        }

        [RelayCommand]
        private async Task RefreshCameras()
        {
            // Run on background thread to avoid freezing UI
            var cams = await Task.Run(() => _cameraService.GetAvailableCameras());
            
            AvailableCameras.Clear();
            foreach (var cam in cams) AvailableCameras.Add(cam);
            
            if (AvailableCameras.Count > 0)
                SelectedCamera = AvailableCameras[0];
            else 
                SelectedCamera = null;
        }
        
        private void OnSelectedCameraChanged(string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                // Parse index from "0: CameraName..." or fallback to old "Camera 0"
                var parts = value.Split(':');
                if (parts.Length > 0 && int.TryParse(parts[0], out int index))
                {
                    _cameraService.StartCamera(index);
                }
                else if (int.TryParse(value.Replace("Câmera ", "").Replace("Camera ", ""), out int fallbackIndex))
                {
                    _cameraService.StartCamera(fallbackIndex);
                }
            }
        }

        [RelayCommand]
        private void SelectClientFolder()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                ClientPath = dialog.FolderName;
                _sessionManager.SetClientPath(ClientPath);
                
                // Populate existing sessions
                ExistingSessions.Clear();
                var sessions = _sessionManager.GetExistingSessions();
                foreach(var s in sessions) ExistingSessions.Add(s);
            }
        }

        [RelayCommand]
        private void GenerateFolder()
        {
             // Extended Validation: OS XXXX or OS XXXX-anything
             if (string.IsNullOrWhiteSpace(OSNumber) || !System.Text.RegularExpressions.Regex.IsMatch(OSNumber, @"^OS\s\d{4}(-[a-zA-Z0-9]+)?$"))
             {
                 System.Windows.MessageBox.Show("Formato INVÁLIDO.\nFormatos aceitos:\n- OS 1234\n- OS 1234-TEXTO", "Erro de Validação", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                 return;
             }
             
             try {
                if(Directory.Exists(ClientPath))
                {
                    string path = System.IO.Path.Combine(ClientPath, OSNumber);
                    if(!Directory.Exists(path)) Directory.CreateDirectory(path);
                    System.Windows.MessageBox.Show($"Pasta verificada: {path}", "Sucesso");
                }
             } catch(Exception ex) {
                 System.Windows.MessageBox.Show($"Erro: {ex.Message}");
             }
        }

        private string _renameOSText = "";
        public string RenameOSText
        {
            get => _renameOSText;
            set => SetProperty(ref _renameOSText, value);
        }

        private bool _isResumedSession = false;

        [RelayCommand]
        private void ToggleSession()
        {
            if (IsSessionActive)
            {
                // End Session Logic
                
                // Check for Rename request ONLY if it was NOT a resumed session (NEW Session)
                // And if Rename field is populated
                if (!_isResumedSession && !string.IsNullOrWhiteSpace(RenameOSText))
                {
                    // Validate New Name
                    if (!System.Text.RegularExpressions.Regex.IsMatch(RenameOSText, @"^OS\s\d{4}(-[a-zA-Z0-9]+)?$"))
                    {
                         System.Windows.MessageBox.Show("Formato para renomear INVÁLIDO.\nFormatos: OS 1234 ou OS 1234-TEXTO", "Erro");
                         return; // Do not close session yet
                    }

                    var result = System.Windows.MessageBox.Show(
                        $"Deseja renomear a pasta de '{OSNumber}' para '{RenameOSText}' e seus arquivos?",
                        "Confirmar Renomeação",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Question);

                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        try
                        {
                            _sessionManager.RenameSession(RenameOSText);
                            System.Windows.MessageBox.Show("Renomeado com sucesso!", "Sucesso");
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show($"Erro ao renomear: {ex.Message}", "Erro");
                             // Can chose to return here or force close.
                             // Safest is to return and let user correct it.
                             return;
                        }
                    }
                }

                _sessionManager.EndSession();
                IsSessionActive = false;
                
                // Reset Fields
                OSNumber = "OS XXXX";
                RenameOSText = "";
                TagText = "";
                GalleryPhotos.Clear();
                SelectedExistingSession = null;
                _isResumedSession = false;
            }
            else
            {
                // Start Session
                try
                {
                    // If user selected an existing session, use it.
                    // If SelectedExistingSession is NOT null/empty, it's a RESUME.
                    
                    if (!string.IsNullOrEmpty(SelectedExistingSession))
                    {
                         // Force OS Number to match selection just in case
                         OSNumber = SelectedExistingSession;
                         _isResumedSession = true;
                    }
                    else 
                    {
                         // New Session
                         _isResumedSession = false;

                         if (string.IsNullOrWhiteSpace(OSNumber) || !System.Text.RegularExpressions.Regex.IsMatch(OSNumber, @"^OS\s\d{4}(-[a-zA-Z0-9]+)?$"))
                        {
                            System.Windows.MessageBox.Show("Formato INVÁLIDO.\nFormatos aceitos:\n- OS 1234\n- OS 1234-TEXTO", "Atenção");
                            return;
                        }
                    }
                    
                    _sessionManager.StartSession(OSNumber);
                    // Clear Resume selection if we started (visual cleanup?) or keep it?
                    // Better to keep it or maybe disable it during session. UI disables inputs anyway.
                    
                    IsSessionActive = true;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Erro ao Iniciar");
                }
            }
        }

        [RelayCommand]
        private void CapturePhoto()
        {
            if (!IsSessionActive || CameraFeed == null) return;

            try
            {
                // Save photo
                string savedPath = _sessionManager.SavePhoto((BitmapSource)CameraFeed, TagText);
                
                // Add to gallery with path
                var clone = ((BitmapSource)CameraFeed).Clone();
                clone.Freeze();
                
                GalleryPhotos.Insert(0, new Models.GalleryItem(clone, savedPath));
            }
            catch (Exception ex)
            {
                 System.Windows.MessageBox.Show($"Falha ao salvar: {ex.Message}");
            }
        }

        [RelayCommand]
        private void DeletePhoto(Models.GalleryItem? item)
        {
            if(item == null) return;
            
            try 
            {
                _sessionManager.DeletePhoto(item.FilePath);
                GalleryPhotos.Remove(item);
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"Erro ao deletar: {ex.Message}");
            }
        }

        [RelayCommand]
        private void OpenHelp()
        {
            var helpWindow = new HelpWindow();
            helpWindow.ShowDialog();
        }
    }
}
