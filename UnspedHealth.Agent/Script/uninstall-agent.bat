@echo off
chcp 65001 > nul

SET SERVICE_NAME="UGM Health Agent"

echo ======================================================
echo UnspedHealth Agent Kaldırma İşlemi Başlıyor
echo ======================================================
echo.

echo [1/2] Servis durduruluyor: %SERVICE_NAME%
sc.exe stop %SERVICE_NAME% > nul

timeout /t 2 /nobreak > nul

echo [2/2] Servis siliniyor...
sc.exe delete %SERVICE_NAME%

echo.
if %errorlevel% equ 0 (
    echo [OK] Servis başarıyla kaldırıldı.
) else (
    echo [HATA] Servis kaldırılırken bir sorun oluştu veya servis zaten yüklü değil.
)

echo.
pause