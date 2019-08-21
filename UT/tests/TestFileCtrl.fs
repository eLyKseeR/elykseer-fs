(*
    eLyKseeR or LXR - cryptographic data archiving software
    https://github.com/eLyKseeR/elykseer-fs
    Copyright (C) 2017-2019 Alexander Diemand

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*)

module TestFileCtrl

open System
open System.IO
open NUnit.Framework
open eLyKseeR

[<Test>]
let ``file does not exist``() =
    Assert.IsFalse(FileCtrl.fileExists("/aasdfasdf_9423jkldkl.dkj3i42"))

[<Test>]
let ``file exists``() =
#if compile_for_windows
    Assert.True(FileCtrl.fileExists(@"C:\Windows\regedit.exe"))
#else
    Assert.IsTrue(FileCtrl.fileExists("/bin/sh"))
#endif

[<Test>]
let ``dir exists``() =
#if compile_for_windows
    Assert.True(FileCtrl.dirExists(@"C:\Windows"))
#else
    Assert.IsTrue(FileCtrl.dirExists("/bin"))
#endif

[<Test>]
let ``dir does not exist - nonexistant``() =
    Assert.IsFalse(FileCtrl.dirExists("/home/Jack_the_ripper_9403094"))

[<Test>]
let ``dir does not exist - a file``() =
#if compile_for_windows
    Assert.False(FileCtrl.dirExists(@"C:\Windows\regedit.exe"))
#else
    Assert.IsFalse(FileCtrl.dirExists("/bin/sh"))
#endif

// [<Test>]
// let ``file date``() =
// #if compile_for_windows
//     let fname = @"C:\Windows\regedit.exe"
// #else
//     let fname = "/bin/sh"
// #endif
//     let fd = FileCtrl.fileDate(fname) in
// #if compile_for_windows
//     Assert.AreEqual(fd, "20090714 03:39:29")
// #endif
// #if compile_for_linux
//     Assert.AreEqual(fd, "20190114 11:37:31")
// #endif
// #if compile_for_osx
//     Assert.AreEqual(fd, "20190504 09:05:03")
// #endif

// [<Test>]
// let ``file size``() =
// #if compile_for_windows
//     let fname = @"C:\Windows\regedit.exe"
// #endif
// #if compile_for_linux
//     let fname = "/bin/bash"
// #endif
// #if compile_for_osx
//     let fname = "/bin/sh"
// #endif
//     let fs = FileCtrl.fileSize(fname) in
// #if compile_for_windows
//     Assert.AreEqual(427008, fs)
// #endif
// #if compile_for_linux
//     Assert.AreEqual(1166912, fs)
// #endif
// #if compile_for_osx
//     Assert.AreEqual(618480, fs)
// #endif

[<Test>]
let ``recursive file listing``() =
    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
#if compile_for_windows
    let fname = @"C:\Windows\System32"
#else
    let fname = "/usr/share"
#endif
    let fps = FileCtrl.fileListRecursive(fname) in
    let lfps = List.length fps in
    Assert.Greater(lfps, 1000)
    stopWatch.Stop()
    let ms = stopWatch.Elapsed.TotalMilliseconds in
    printfn "time needed: %f ms for %d entries\n" ms lfps
    printfn " => %f ms per entry, %f ms per 1000" (ms / (double lfps)) (ms * 1000.0 / (double lfps))
