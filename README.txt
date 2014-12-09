
AutoDECK is a small automation tool for "RUNDECK(rundeck.org/)" made with .NET(C#) ....... Basically it does the setup work for you from 5 to 5000 hosts! ;) 

DEPS: 
You will need the full "Putty" install and "Java 1.6 (for Rundeck)" on a Windows system.

#JAVA
http://www.oracle.com/technetwork/java/javase/downloads/java-archive-downloads-javase6-419409.html#jre-6u45-oth-JPR
http://www.oracle.com/technetwork/java/javase/archive-139210.html

#PUTTY (Use The Full Installer)
http://the.earth.li/~sgtatham/putty/latest/x86/putty-0.63-installer.exe
http://www.chiark.greenend.org.uk/~sgtatham/putty/download.html



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

[[ "SSHProtocolFailure: Algorithm negotiation fail" -> RUNDECK ERROR ]]
This seems to be a known issue with SSH-1.99-OpenSSH_6.7 as it uses a newer "Diffie-Hellman group exchange"; Most of my hosts had SSH-1.99-OpenSSH_6.4 and below.... and work fine.


#RUNDECK uses "jsch" as for the SSH protocal... 
http://www.jcraft.com/jsch/

It does not seem to support the newer SSH2 protocol key exchange:

Key exchange: diffie-hellman-group-exchange-sha1, diffie-hellman-group1-sha1
Cipher: blowfish-cbc,3des-cbc,aes128-cbc,aes192-cbc,aes256-cbc,aes128-ctr,aes192-ctr,aes256-ctr,3des-ctr,arcfour,arcfour128,arcfour256
MAC: hmac-md5, hmac-sha1, hmac-md5-96, hmac-sha1-96
Host key type: ssh-dss,ssh-rsa 


#AN SSH DEBUG
C:\>plink -i C:\id_dsa.ppk -ssh -v root@14.0.0.126
Looking up host "14.0.0.126"
Connecting to 14.0.0.126 port 22
Server version: SSH-1.99-OpenSSH_6.7
Using SSH protocol version 2
We claim version: SSH-2.0-PuTTY_Release_0.63
Doing Diffie-Hellman group exchange
Doing Diffie-Hellman key exchange with hash SHA-256 <=============== (jsch) JAVA SSH DOES NOT LIKE THIS [http://www.jcraft.com/jsch/]
Host key fingerprint is:
ssh-rsa 2048 90:6a:2b:60:2f:cd:46:cc:14:05:26:fc:b7:3e:6a:2d
Initialised AES-256 SDCTR client->server encryption
Initialised HMAC-SHA-256 client->server MAC algorithm
Initialised AES-256 SDCTR server->client encryption
Initialised HMAC-SHA-256 server->client MAC algorithm
Reading private key file "C:\id_dsa.ppk"
Using username "root".
Offered public key
Offer of public key accepted
Authenticating with public key "rsa-key-20141204"
Sent public key signature
Access granted
Opening session as main channel
Opened main channel
Allocated pty (ospeed 38400bps, ispeed 38400bps)
C:\>









