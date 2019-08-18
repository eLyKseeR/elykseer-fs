#!/bin/bash

set -e

PLATFRM="Any CPU"

nuget restore -PackagesDirectory packages packages.config
nuget restore -PackagesDirectory packages UT/packages.config

# make dependencies
cd ext/openssl-net.git/; ./build.Linux.jenkins.sh
cd ../..
cd ext/prngsharp.git/; ./build.Linux.jenkins.sh
cd ../..


# build
sn -k base/eLyKseeR.snk
sn -k native/eLyKseeR-native.snk
sn -k UT/ut.snk

xbuild /t:clean /p:Configuration="Debug" /p:Platform="${PLATFRM}" base/eLyKseeR-base.Linux.sln
xbuild /p:Configuration="Debug" /p:Platform="${PLATFRM}" base/eLyKseeR-base.Linux.sln

mono packages/NUnit.ConsoleRunner.3.10.0/tools/nunit3-console.exe UT/bin/Debug/UT.exe

# NUnit test results in: TestResult.xml

sn -k cli/LXRbackup/LXRbackup.snk
sn -k cli/LXRrestore/LXRrestore.snk

xbuild /t:clean /p:Configuration="Debug" /p:Platform="${PLATFRM}" cli/elykseer-cli.Mono.sln
xbuild /p:Configuration="Debug" /p:Platform="${PLATFRM}" cli/elykseer-cli.Mono.sln

sn -k gui/LXRbackup/LXRbackup.snk
sn -k gui/LXRrestore/LXRrestore.snk

xbuild /t:clean /p:Configuration="Debug" /p:Platform="${PLATFRM}" gui/LXR_GUIs.Mono.sln
xbuild /p:Configuration="Debug" /p:Platform="${PLATFRM}" gui/LXR_GUIs.Mono.sln
