@echo off
setlocal
set "PCLPLUSS_TARGET=%~dp0"
echo ============================================
echo   PCLPLUSS 解除文件锁定工具
echo ============================================
echo.
echo 正在解除当前文件夹及其子文件夹中所有文件的
echo 「来自互联网」锁定标记（Mark-of-the-Web）...
echo.
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-ChildItem -LiteralPath $env:PCLPLUSS_TARGET -Recurse -File -ErrorAction SilentlyContinue | Unblock-File -ErrorAction SilentlyContinue"
echo.
echo 处理完成！现在可以双击 PCLPLUSS.exe 启动。
echo 如果仍然弹出 SmartScreen，点「更多信息」-「仍要运行」即可。
echo.
pause