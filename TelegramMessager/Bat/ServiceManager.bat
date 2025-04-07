chcp 65001 >nul
@echo off
setlocal enabledelayedexpansion

:: Путь к текущей директории
set "SCRIPT_DIR=%~dp0"
set "CONFIG_FILE=%SCRIPT_DIR%config.ini"

:: Проверка наличия конфигурационного файла
if not exist "%CONFIG_FILE%" (
    echo Ошибка: Файл конфигурации не найден: %CONFIG_FILE%
    pause
    goto :EOF
)

:: Чтение конфигурации
for /f "tokens=1,2 delims==" %%a in ('type "%CONFIG_FILE%" ^| findstr /v "^;"') do (
    set "%%a=%%b"
)

:: Проверка наличия обязательных параметров
if not defined RestartInterval (
    echo Ошибка: В config.ini не указан параметр RestartInterval
    pause
    goto :EOF
)
if not defined ResetCounterInterval (
    echo Ошибка: В config.ini не указан параметр ResetCounterInterval
    pause
    goto :EOF
)
if not defined RestartAttempts (
    echo Ошибка: В config.ini не указан параметр RestartAttempts
    pause
    goto :EOF
)

:: Создание директории для логов если указана
if defined LogPath (
    if not exist "%SCRIPT_DIR%%LogPath%" mkdir "%SCRIPT_DIR%%LogPath%"
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
    pause
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
echo 7. Показать логи
echo 8. Очистить логи
echo 9. Выход
echo.
echo Текущие настройки:
echo - Тип программы: %ProgramType%
echo - Интервал перезапуска: %RestartInterval% мс
echo - Сброс счетчика: %ResetCounterInterval% сек
if "%RestartAttempts%"=="-1" (
    echo - Количество попыток: бесконечно
) else (
    echo - Количество попыток: %RestartAttempts%
)
if defined LogPath (
    echo - Путь к логам: %LogPath%
    echo - Макс. размер лога: %MaxLogSize% МБ
    echo - Хранение логов: %LogRetentionDays% дней
)
echo.

set /p choice="Выберите действие (1-9): "

if "%choice%"=="1" goto :install
if "%choice%"=="2" goto :uninstall
if "%choice%"=="3" goto :start
if "%choice%"=="4" goto :stop
if "%choice%"=="5" goto :restart
if "%choice%"=="6" goto :status
if "%choice%"=="7" goto :show_logs
if "%choice%"=="8" goto :clear_logs
if "%choice%"=="9" goto :EOF

goto :menu

:check_service_exists
sc query "%ServiceName%" >nul 2>&1

:: Если служба существует, errorlevel будет 0.
:: Если не существует то errorlevel >= 1
if errorlevel 1 (
    echo Служба "%ServiceName%" не установлена в системе
    exit /b 1
)
exit /b 0

:check_service_running

sc query "%ServiceName%" | findstr "RUNNING" >nul

::errorlevel 1 означает, что команда find не нашла строки "RUNNING"
if errorlevel 1 (
    echo Служба "%ServiceName%" не запущена.
    exit /b 1
)
exit /b 0

:install
echo Проверка службы...
call :check_service_exists
if not errorlevel 1 (
    echo Служба "%ServiceName%" уже установлена
    echo Используйте пункт 3 для запуска службы
    echo Или используйте пункт 2 для удаления службы а после пукт 1 для установки с чистого листа
    pause
    goto :menu
)

echo Установка службы...
if /i "%ProgramType%"=="Core" (
    sc create "%ServiceName%" binPath= "!FULL_EXE_PATH!" displayname= "%DisplayName%" start= auto obj= %ServiceAccount%
) else (
    sc create "%ServiceName%" binPath= "!FULL_EXE_PATH! /service" displayname= "%DisplayName%" start= auto obj= %ServiceAccount% type= own
)
sc description "%ServiceName%" "%Description%"
if "%RestartAttempts%"=="-1" (
    sc failure "%ServiceName%" reset= %ResetCounterInterval% actions= restart/%RestartInterval%/restart/%RestartInterval%/restart/%RestartInterval%
) else (
    sc failure "%ServiceName%" reset= %ResetCounterInterval% actions= restart/%RestartInterval%
)

echo Служба установлена и настроена на автоматический перезапуск
echo - Интервал перезапуска: %RestartInterval% мс
echo - Сброс счетчика: %ResetCounterInterval% сек
if "%RestartAttempts%"=="-1" (
    echo - Количество попыток: бесконечно
) else (
    echo - Количество попыток: %RestartAttempts%
)
pause
goto :menu

:uninstall
echo Проверка что служба установлена...

call :check_service_exists
if errorlevel 1 (
    echo Такой службы найдено не было
    pause
    goto :menu
)

echo Проверка что служба запущена ...
call :check_service_running
if not errorlevel 1 (
    sc stop "%ServiceName%"
)

echo Удаление службы...
sc delete "%ServiceName%"
echo Служба успешно удалена
pause
goto :menu

:start
echo Проверка службы...
call :check_service_exists
if errorlevel 1 (
    echo Не прошёл проверку службы.
    pause
    goto :menu
)

call :check_service_running
if not errorlevel 1 (
    echo уже запущен
    pause
    goto :menu
)

echo Запуск службы...
sc start "%ServiceName%"
if not errorlevel 1 (
    echo Не прошёл проверку службы после запуска
    pause
    goto :menu
)
echo Служба запущена
pause
goto :menu

:stop
echo Проверка службы...
call :check_service_exists
if errorlevel 1 (
    echo Служба "%ServiceName%" не установлена
    echo Нет необходимости в остановке
    pause
    goto :menu
)

call :check_service_running
if errorlevel 1 (
    echo Служба "%ServiceName%" уже остановлена
    pause
    goto :menu
)

echo Остановка службы...
sc stop "%ServiceName%"
echo Служба остановлена
pause
goto :menu

:restart
echo Проверка службы...
call :check_service_exists
if errorlevel 1 (
    echo Служба "%ServiceName%" не установлена
    echo Сначала установите службу (пункт 1)
    pause
    goto :menu
)

echo Перезапуск службы...
sc stop "%ServiceName%"
timeout /t 5 /nobreak > nul
sc start "%ServiceName%"
echo Служба перезапущена
pause
goto :menu

:status
echo Проверка службы...
call :check_service_exists
if errorlevel 1 (
    echo Служба "%ServiceName%" не установлена
    pause
    goto :menu
)

echo Текущий статус службы:
sc query "%ServiceName%"
pause
goto :menu

:show_logs
if not defined LogPath (
    echo Логирование не настроено
    pause
    goto :menu
)

if not exist "%SCRIPT_DIR%%LogPath%" (
    echo Директория логов не найдена
    pause
    goto :menu
)

echo Показ последних логов:
type "%SCRIPT_DIR%%LogPath%\*.log"
pause
goto :menu

:clear_logs
if not defined LogPath (
    echo Логирование не настроено
    pause
    goto :menu
)

if not exist "%SCRIPT_DIR%%LogPath%" (
    echo Директория логов не найдена
    pause
    goto :menu
)

echo Очистка логов...
if defined LogRetentionDays (
    if not "%LogRetentionDays%"=="0" (
        forfiles /p "%SCRIPT_DIR%%LogPath%" /m *.log /d -%LogRetentionDays% /c "cmd /c del @path"
    )
)
if defined MaxLogSize (
    if not "%MaxLogSize%"=="0" (
        for %%f in ("%SCRIPT_DIR%%LogPath%\*.log") do (
            for %%a in ("%%~zf") do set "size=%%~za"
            set /a "size_mb=!size! / 1048576"
            if !size_mb! gtr %MaxLogSize% (
                del "%%f"
            )
        )
    )
)
echo Логи очищены
pause
goto :menu 