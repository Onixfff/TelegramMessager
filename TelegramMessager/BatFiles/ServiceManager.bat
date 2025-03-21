@echo off
setlocal enabledelayedexpansion

:: Путь к текущей директории
set "SCRIPT_DIR=%~dp0"
set "CONFIG_FILE=%SCRIPT_DIR%config.ini"

:: Проверка наличия конфигурационного файла
if not exist "%CONFIG_FILE%" (
    echo Ошибка: Файл конфигурации не найден: %CONFIG_FILE%
    goto :EOF
)

:: Чтение конфигурации
for /f "tokens=1,2 delims==" %%a in ('type "%CONFIG_FILE%" ^| findstr /v "^;"') do (
    set "%%a=%%b"
)

:: Получаем полный путь к exe
if "%ExePath:~0,1%" == "." (
    set "FULL_EXE_PATH=%SCRIPT_DIR%%ExePath%"
) else (
    set "FULL_EXE_PATH=%ExePath%"
)

:: Проверяем существование exe
if not exist "!FULL_EXE_PATH!" (
    echo Ошибка: Исполняемый файл не найден: !FULL_EXE_PATH!
    goto :EOF
)

:menu
cls
echo ============================================
echo Управление службой %ServiceName%
echo ============================================
echo 1. Установить службу
echo 2. Удалить службу
echo 3. Запустить службу
echo 4. Остановить службу
echo 5. Перезапустить службу
echo 6. Проверить статус
echo 7. Выход
echo.

set /p choice="Выберите действие (1-7): "

if "%choice%"=="1" goto :install
if "%choice%"=="2" goto :uninstall
if "%choice%"=="3" goto :start
if "%choice%"=="4" goto :stop
if "%choice%"=="5" goto :restart
if "%choice%"=="6" goto :status
if "%choice%"=="7" goto :EOF

goto :menu

:install
echo Установка службы...
if /i "%ProgramType%"=="Core" (
    sc create "%ServiceName%" binPath= "!FULL_EXE_PATH!" displayname= "%DisplayName%" start= %StartupType% obj= %ServiceAccount%
    sc description "%ServiceName%" "%Description%"
) else (
    sc create "%ServiceName%" binPath= "!FULL_EXE_PATH!" displayname= "%DisplayName%" start= %StartupType% obj= %ServiceAccount%
    sc description "%ServiceName%" "%Description%"
)
pause
goto :menu

:uninstall
echo Удаление службы...
sc stop "%ServiceName%"
sc delete "%ServiceName%"
pause
goto :menu

:start
echo Запуск службы...
sc start "%ServiceName%"
pause
goto :menu

:stop
echo Остановка службы...
sc stop "%ServiceName%"
pause
goto :menu

:restart
echo Перезапуск службы...
sc stop "%ServiceName%"
timeout /t 5 /nobreak > nul
sc start "%ServiceName%"
pause
goto :menu

:status
echo Проверка статуса службы...
sc query "%ServiceName%"
pause
goto :menu 