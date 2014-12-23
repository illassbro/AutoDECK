
AutoDECK is a small automation tool for "RUNDECK(rundeck.org/)" made with .NET(C#) ....... Basically it does the setup work for you from 5 to 5000 hosts! ;) 

s
BUILD AutoDECK
======================

#Here is how you would build it... with my quick .NET tool chain.
Download latest SSH.NET(if need be): https://sshnet.codeplex.com/

:[[ FROM THE CLI USING THE .NET FRAMEWORK NATIVE COMPILER ]]

set PATH /p
:#USE .NET VERSIONS
:set PATH=C:\Windows\Microsoft.NET\Framework\v2.0.50727\;%PATH%
:set PATH=C:\Windows\Microsoft.NET\Framework\v3.0\;%PATH%
:set PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v3.5\;%PATH%
:
set PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\;%PATH%
set PATH /p

csc.exe -version

:C# QUICK COMPILE/REPL 
for /F "tokens=1-3 delims=: " %i in ('time /t') do notepad AutoDECK-beta-clean.cs && pause && csc.exe /out:AutoDECK.exe /r:Renci.SshNet.dll AutoDECK-beta-clean.cs && cp AutoDECK-beta-clean.cs AutoDECK-beta-clean.cs.%i%j%k && AutoDECK.exe



The Code uses "SSH.NET"
======================

New BSD License (BSD)

Copyright (c) 2010, RENCI
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
* Neither the name of RENCI nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

http://sshnet.codeplex.com/license






