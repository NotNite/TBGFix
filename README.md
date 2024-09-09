# TBGFix

Old versions of the Portal 2 branch of Source 1 has issues with large amounts of CPU cores. This issue affects [The Beginner's Guide](https://store.steampowered.com/app/303210/The_Beginners_Guide/). I found [portal2start](https://github.com/b-desconocido/portal2start) which fixes this by modifying kernel32's GetSystemInfo, and reimplemented it in C#.
