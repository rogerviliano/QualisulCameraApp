Add-Type -AssemblyName System.Drawing
$sourcePath = "icon_source.png"
$destPath = "app_icon.ico"

$bitmap = [System.Drawing.Bitmap]::FromFile($sourcePath)
# Create a new bitmap with standard icon size if needed, or just use as is if square.
# Let's resize to standard 256x256 for better compatibility
$resized = new-object System.Drawing.Bitmap 256, 256
$g = [System.Drawing.Graphics]::FromImage($resized)
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.DrawImage($bitmap, 0, 0, 256, 256)
$g.Dispose()

$icon = [System.Drawing.Icon]::FromHandle($resized.GetHicon())
$fileStream = New-Object System.IO.FileStream $destPath, 'Create'
$icon.Save($fileStream)
$fileStream.Close()
$icon.Dispose()
$resized.Dispose()
$bitmap.Dispose()

Write-Host "Icon created at $destPath"
