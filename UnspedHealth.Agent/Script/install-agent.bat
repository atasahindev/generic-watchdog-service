@echo off
chcp 65001 > nul

SET SERVICE_NAME="UGM Health Agent"
SET BIN_PATH="C:\Services\UGMHealthAgentService\UnspedHealth.Agent.exe"
SET DESCRIPTION="Unsped Health Watchdog Sistemi Uzak Yönetim Ajanı"

echo [1/3] Servis oluşturuluyor: %SERVICE_NAME%
sc.exe create %SERVICE_NAME% binPath= %BIN_PATH% start= auto

echo [2/3] Servis açıklaması ekleniyor...
sc.exe description %SERVICE_NAME% %DESCRIPTION%

echo [3/3] Servis başlatılıyor...
sc.exe start %SERVICE_NAME%

echo.
echo Kurulum başarıyla tamamlandı.
pause