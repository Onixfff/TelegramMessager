@echo off
setlocal enabledelayedexpansion

:: Путь к config.ini
set "CONFIG_FILE=..\config.ini"

echo "%CONFIG_FILE%"

:: Читаем LogPath из config.ini
for /f "tokens=2 delims==" %%a in ('findstr /i "^LogPath=" "%CONFIG_FILE%"') do set "LOG_PATH=%%a"

:: Убираем кавычки (если есть)
set "LOG_PATH=%LOG_PATH:"=%"

:: Вызываем PowerShell для редактирования XML
powershell -ExecutionPolicy Bypass -NoProfile -File update_nlog.ps1 "%LOG_PATH%"

echo Готово! Значение LogDirectory обновлено в NLog.config.
pause