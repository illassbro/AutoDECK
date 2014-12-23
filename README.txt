
AutoDECK is a small automation tool for "RUNDECK(rundeck.org/)" made with .NET(C#) ....... Basically it does the setup work for you from 5 to 5000 hosts! ;) 

DEPS: 
You will need "Java 1.6 (for Rundeck)" on a Windows system.

#JAVA
http://www.oracle.com/technetwork/java/javase/downloads/java-archive-downloads-javase6-419409.html#jre-6u45-oth-JPR
http://www.oracle.com/technetwork/java/javase/archive-139210.html



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



Known Problems
======================
Known Issues (AutoDECK):

One thing to note is that the tool will try all known password on every host; so if you have one password 
for every host it will be fine but if you have different passwords for each host try to make the two files 
(hostfile/passfile) match line for line else it will try passwords in the list until it finds one that works
this could take a long time depending on how many password you have.


Known Issues (Rundeck/jsch):

[[ "SSHProtocolFailure: Algorithm negotiation fail" -> RUNDECK ERROR ]]
This seems to be a known issue with SSH-1.99-OpenSSH_6.7 as it uses a newer "Diffie-Hellman group exchange" 
Most of my hosts had SSH-1.99-OpenSSH_6.4 and below.... and work fine.

RUNDECK uses "jsch" as for the SSH protocol... "jsch" does not seem to support the newer SSH2 protocol key exchange yet.
http://www.jcraft.com/jsch/









