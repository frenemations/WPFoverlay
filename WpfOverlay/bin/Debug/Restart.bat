:RETRY

TASKKILL /im WpfOverlay.exe
IF %errorlevel% == 128 (
  ECHO Overlay Still Running
  sleep 1
  GOTO RETRY
) ELSE (
  
GOTO CONTINUE
)

:CONTINUE

START WpfOverlay.exe


