# Qualisul Camera App

Aplicação desktop Windows para captura de fotos profissional com organização por Ordem de Serviço (OS).

## 🎯 Características

- 📸 **Captura de Fotos**: Suporte para webcams e câmeras de celular via DroidCam/Iriun
- 📁 **Organização Automática**: Sistema de pastas por OS com numeração sequencial
- 🖼️ **Galeria Integrada**: Visualização e gestão de fotos capturadas
- 🏷️ **Sistema de Tags**: Marcação de peças/componentes específicos
- 🔄 **Retomada de Sessões**: Continue sessões anteriores mantendo a sequência
- ✏️ **Renomeação de Sessões**: Corrija o nome da OS após captura
- 🎨 **Interface Dark**: Tema escuro Qualisul (Azul #1A2639 + Laranja)
- ❓ **Sistema de Ajuda**: Procedimentos integrados na aplicação

## 🚀 Como Usar

### Instalação
1. Baixe o executável `QualisulCameraApp.exe`
2. Execute (não requer instalação)

### Configuração Inicial
1. Conecte uma webcam OU configure DroidCam no celular
2. Selecione a pasta do cliente
3. Defina a Ordem de Serviço

### Fluxo de Trabalho
1. **INICIAR SESSÃO** → Capturar fotos → **ENCERRAR SESSÃO**
2. Use o campo TAG para identificar componentes específicos
3. Exclua fotos indesejadas clicando no ícone da lixeira
4. Renomeie a sessão se necessário (antes de encerrar)

## 🔧 Tecnologias

- **.NET 8** (Windows WPF)
- **OpenCvSharp4**: Captura e processamento de vídeo
- **CommunityToolkit.Mvvm**: Padrão MVVM
- **System.Management**: Detecção de câmeras

## 📱 Usando Câmera do Celular

1. Instale [DroidCam](https://www.dev47apps.com/droidcam/windows/) no PC e celular
2. Conecte via Wi-Fi ou USB
3. Clique em "Atualizar" no app Qualisul
4. Selecione "DroidCam Source"

## 📦 Build

```powershell
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true
```

O executável será gerado em: `bin/Release/net8.0-windows/win-x64/publish/`

## 📄 Licença

© 2024 Qualisul Metrologia. Todos os direitos reservados.
