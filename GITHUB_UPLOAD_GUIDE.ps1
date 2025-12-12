# Guia de Upload para GitHub - Qualisul Camera App
# Execute estes comandos um por um no PowerShell

# 1. Navegue até a pasta do projeto
cd "C:\Users\roger.viliano.QUALISUL\.gemini\antigravity\playground\triple-newton\QualisulCameraApp"

# 2. Inicialize o repositório Git
git init

# 3. Adicione todos os arquivos (exceto os ignorados no .gitignore)
git add .

# 4. Crie o primeiro commit
git commit -m "Initial commit: Qualisul Camera App v2.1"

# 5. Renomeie a branch para 'main' (padrão do GitHub)
git branch -M main

# 6. Adicione o repositório remoto do GitHub
# SUBSTITUA 'QualisulCameraApp' pelo nome EXATO do seu repositório se for diferente
git remote add origin https://github.com/rogerviliano/QualisulCameraApp.git

# 7. Envie para o GitHub
git push -u origin main

# NOTA: Você será solicitado a fazer login no GitHub na primeira vez
# Use suas credenciais do GitHub quando aparecer a janela de autenticação
