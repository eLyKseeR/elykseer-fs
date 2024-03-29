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

module TestBackupCtrl

open NUnit.Framework
open eLyKseeR
open System.IO

let dir = "./obj"
[<Test>]
let ``start and calculate free space``() =
    let o1 = new Options()
    o1.setNchunks 16
    o1.setRedundancy 0
    o1.setFpathDb dir
    o1.setFpathChunks dir

    Logging.enable_console ()

    let b1 = BackupCtrl.create o1
    Assert.AreEqual(0, BackupCtrl.bytes_in b1)
    Assert.AreEqual(0, BackupCtrl.bytes_out b1)
    Assert.AreEqual(0, BackupCtrl.time_encrypt b1)
    Assert.AreEqual(0, BackupCtrl.time_extract b1)
    Assert.AreEqual(0, BackupCtrl.time_write b1)
    Assert.AreEqual(BackupCtrl.free b1, (16 * 256 * 1024 - 33))

[<Test>]
let ``backup some file``() =
    let o1 = new Options()
    o1.setNchunks 16
    o1.setRedundancy 0
    o1.setFpathDb dir
    o1.setFpathChunks dir

    Logging.enable_console ()

    let b1 = BackupCtrl.create o1
#if compile_for_windows
    let fname = @"C:\Windows\regedit.exe"
#endif
#if compile_for_linux
    let fname = "/bin/bash"
    #endif
#if compile_for_osx
    let fname = "/bin/sh"
#endif
    let fsize = FileCtrl.fileSize fname

    BackupCtrl.backup b1 fname

    Assert.AreEqual(fsize, BackupCtrl.bytes_in b1)
    Assert.AreEqual(fsize, BackupCtrl.bytes_out b1)
    Assert.AreEqual(0, BackupCtrl.time_encrypt b1)
    Assert.AreEqual(0, BackupCtrl.time_extract b1)
    Assert.Greater(BackupCtrl.time_write b1, 0)
    Assert.AreEqual(BackupCtrl.free b1, (16 * 256 * 1024 - 33 - (int fsize)))

[<Test>]
let ``backup another file with compression and watch timing``() =
    let o1 = new Options()
    o1.setNchunks 16
    o1.setRedundancy 0
    o1.setFpathDb dir
    o1.setFpathChunks dir
    o1.setCompression true

    Logging.enable_console ()

    let b1 = BackupCtrl.create o1
#if compile_for_windows
    let fnames = [@"C:\Windows\notepad.exe"; @"C:\Windows\regedit.exe"]
#endif
#if compile_for_linux
    let fnames = ["/usr/bin/zip"; "/bin/bash"]
#endif
#if compile_for_osx
    let fnames = ["/usr/bin/zip"; "/bin/sh"]
#endif
    let mutable fsizes = 0;

    for fname in fnames do
        let fsize = FileCtrl.fileSize fname
        fsizes <- fsizes + (int)fsize
        BackupCtrl.backup b1 fname

    BackupCtrl.finalize b1

    let bi = BackupCtrl.bytes_in b1
    let bo = BackupCtrl.bytes_out b1
    Printf.printfn "we have been reading %d bytes and writing %d bytes\ncompression rate: %f%%" bi bo (100.0 * (1.0 - double(bo)/double(bi)))
    Assert.AreEqual(fsizes, bi)
    Assert.GreaterOrEqual(fsizes, bo)
    Assert.Greater(BackupCtrl.time_encrypt b1, 0)
    Assert.Greater(BackupCtrl.time_extract b1, 0)
    Assert.Greater(BackupCtrl.time_write b1, 0)
    //Assert.AreEqual(BackupCtrl.free b1, (16 * 256 * 1024 - 33 - (int fsize)))

    use tw = new StreamWriter( System.Console.OpenStandardOutput() )
    (BackupCtrl.getDbFp b1).outStream tw
    (BackupCtrl.getDbKeys b1).outStream tw
    tw.Flush()

[<Test>]
let ``backup a file twice and watch deduplication at level 1``() =
    let o1 = new Options()
    o1.setNchunks 16
    o1.setRedundancy 0
    o1.setFpathDb dir
    o1.setFpathChunks dir
    o1.setCompression true
    o1.setDeduplication 1

    Logging.enable_console ()

#if compile_for_windows
    let fnames = [@"C:\Windows\notepad.exe"; @"C:\Windows\regedit.exe"]
#else
    let fnames = ["/usr/bin/zip"; "/bin/sh"]
#endif
    let b1 = BackupCtrl.create o1
    try
        for fname in fnames do
            BackupCtrl.backup b1 fname
    with
    | BackupCtrl.BadAccess fp -> System.Console.WriteLine("exception in {0}", fp)
    | e -> System.Console.WriteLine("exception {0}", e.ToString())

    BackupCtrl.finalize b1

    let bi1 = BackupCtrl.bytes_in b1
    let bo1 = BackupCtrl.bytes_out b1
    Printf.printfn "1st run: we have been reading %d bytes and writing %d bytes\ncompression rate: %f%%" bi1 bo1 (100.0 * (1.0 - double(bo1)/double(bi1)))

    use tw1 = new StreamWriter( System.Console.OpenStandardOutput() )
    (BackupCtrl.getDbFp b1).outStream tw1
    (BackupCtrl.getDbKeys b1).outStream tw1
    tw1.Flush()

    (* new backup with previous backup db set as reference *)
    let b2 = BackupCtrl.create o1
    BackupCtrl.setReference b2 (Some (BackupCtrl.getDbKeys b1)) (Some (BackupCtrl.getDbFp b1))
#if compile_for_windows
    for fname in [@"C:\Windows\notepad.exe"; @"C:\Windows\SysWOW64\compmgmt.msc"; @"C:\Windows\regedit.exe"] do
#else
    for fname in ["/usr/bin/zip"; "/usr/bin/zip"; "/bin/sh"] do
#endif
        BackupCtrl.backup b2 fname
    BackupCtrl.finalize b2

    let bi2 = BackupCtrl.bytes_in b2
    let bo2 = BackupCtrl.bytes_out b2
    Printf.printfn "2nd run: we have been reading %d bytes and writing %d bytes\ncompression rate: %f%%" bi2 bo2 (100.0 * (1.0 - double(bo2)/double(bi2)))

    use tw2 = new StreamWriter( System.Console.OpenStandardOutput() )
    (BackupCtrl.getDbFp b2).outStream tw2
    (BackupCtrl.getDbKeys b2).outStream tw2
    tw2.Flush()

#if compile_for_windows
    let queryfname = @"C:\Windows\notepad.exe"
#else
    let queryfname = "/bin/sh"
#endif
    let dat1 = (BackupCtrl.getDbFp b1).idb.get queryfname
    System.Console.WriteLine("1st: {0}", dat1)
    let dat2 = (BackupCtrl.getDbFp b2).idb.get queryfname
    System.Console.WriteLine("2nd: {0}", dat2)

    (BackupCtrl.getDbFp b1).idb.get queryfname
        |> Option.map (fun (e : DbFpDat) -> for b in e.blocks do System.Console.WriteLine("1st: {0}={1}",b.idx,Key256.toHex b.aid))
        |> ignore
    (BackupCtrl.getDbFp b2).idb.get queryfname
        |> Option.map (fun (e : DbFpDat) -> for b in e.blocks do System.Console.WriteLine("2nd: {0}={1}",b.idx,Key256.toHex b.aid))
        |> ignore

[<Test>]
let ``backup some files which do not fit into a single assembly``() =
    let o1 = new Options()
    o1.setNchunks 16
    o1.setRedundancy 0
    o1.setFpathDb dir
    o1.setFpathChunks dir
    o1.setCompression true

    Logging.enable_console ()

    let b1 = BackupCtrl.create o1
    let mutable fsize = 0L
#if compile_for_windows
    for fname in [@"C:\Windows\explorer.exe"; @"C:\Windows\SysWOW64\compmgmt.msc"; @"C:\Windows\regedit.exe"] do
#endif
#if compile_for_linux
    for fname in ["/usr/bin/gdb";"/usr/bin/curl";"/usr/bin/diff";"/usr/bin/ssh"] do
#endif
#if compile_for_osx
    for fname in ["/usr/bin/zip";"/usr/bin/host";"/usr/bin/ssh"] do
#endif
    if FileCtrl.fileExists(fname) then
        fsize <- fsize + FileCtrl.fileSize fname
        BackupCtrl.backup b1 fname

    Assert.AreEqual(fsize, BackupCtrl.bytes_in b1)
    Assert.GreaterOrEqual(fsize, BackupCtrl.bytes_out b1)
    Assert.GreaterOrEqual(BackupCtrl.time_encrypt b1, 0)
    Assert.GreaterOrEqual(BackupCtrl.time_extract b1, 0)
    Assert.GreaterOrEqual(BackupCtrl.time_write b1, 0)

    BackupCtrl.finalize b1

    use tw1 = new StreamWriter( System.Console.OpenStandardOutput() )
    (BackupCtrl.getDbFp b1).outStream tw1
    (BackupCtrl.getDbKeys b1).outStream tw1
    tw1.Flush()

    let bi = BackupCtrl.bytes_in b1
    let bo = BackupCtrl.bytes_out b1
    Printf.printfn "we have been reading %d bytes and writing %d bytes\ncompression rate: %f%%" bi bo (100.0 * (1.0 - double(bo)/double(bi)))

[<Test>]
let ``backup a file twice and watch deduplication at level 2``() =
    let o1 = new Options()
    o1.setNchunks 16
    o1.setRedundancy 0
    o1.setFpathDb dir
    o1.setFpathChunks dir
    o1.setCompression true
    o1.setDeduplication 2

    Logging.enable_console ()

    let b1 = BackupCtrl.create o1
    try
    #if compile_for_windows
        for fname in [@"C:\Windows\explorer.exe"; @"C:\Windows\regedit.exe"] do
    #else
        for fname in ["/usr/bin/zip"; "/bin/sh"] do
    #endif
            BackupCtrl.backup b1 fname
        BackupCtrl.finalize b1
    with
    | e -> System.Console.WriteLine("exception: {0}", e)

    let bi1 = BackupCtrl.bytes_in b1
    let bo1 = BackupCtrl.bytes_out b1
    Logging.log () <| Printf.sprintf "1st run: we have been reading %d bytes and writing %d bytes\ncompression rate: %.2f%%" bi1 bo1 (100.0 * (1.0 - double(bo1)/double(bi1)))

    use tw1 = new StreamWriter( System.Console.OpenStandardOutput() )
    (BackupCtrl.getDbFp b1).outStream tw1
    (BackupCtrl.getDbKeys b1).outStream tw1
    tw1.Flush()

    (* new backup with previous backup db set as reference *)
    let b2 = BackupCtrl.create o1
    BackupCtrl.setReference b2 (Some (BackupCtrl.getDbKeys b1)) (Some (BackupCtrl.getDbFp b1))
#if compile_for_windows
    for fname in [@"C:\Windows\explorer.exe"; @"C:\Windows\write.exe"; @"C:\Windows\regedit.exe"] do
#else
    for fname in ["/usr/bin/zip"; "/usr/bin/zip"; "/bin/sh"] do
#endif
        BackupCtrl.backup b2 fname
    BackupCtrl.finalize b2

    let bi2 = BackupCtrl.bytes_in b2
    let bo2 = BackupCtrl.bytes_out b2
    Printf.printfn "2nd run: we have been reading %d bytes and writing %d bytes\ncompression rate: %.2f%%" bi2 bo2 (100.0 * (1.0 - double(bo2)/double(bi2)))

    use tw2 = new StreamWriter( System.Console.OpenStandardOutput() )
    (BackupCtrl.getDbFp b2).outStream tw2
    (BackupCtrl.getDbKeys b2).outStream tw2
    tw2.Flush()

#if compile_for_windows
    let qfname = @"C:\Windows\regedit.exe"
#else
    let qfname = "/bin/sh"
#endif
    let test1 = match (BackupCtrl.getDbFp b1).idb.get qfname with
                | None -> raise <| System.Exception "very bad 1!"
                | Some (e : DbFpDat) ->
                    List.sortBy (fun b -> b.idx) e.blocks
                    |> List.fold (fun acc b -> acc + (Printf.sprintf "%d=%s" b.idx (Key256.toHex b.aid))) ""
    let test2 = match (BackupCtrl.getDbFp b2).idb.get qfname with
                | None -> raise <| System.Exception "very bad 2!"
                | Some (e : DbFpDat) ->
                    List.sortBy (fun b -> b.idx) e.blocks
                    |> List.fold (fun acc b -> acc + (Printf.sprintf "%d=%s" b.idx (Key256.toHex b.aid))) ""

    Assert.AreEqual(test1,test2)

[<Test>]
let ``backup a file twice, append to it, and watch deduplication at level 2``() =
    let o1 = new Options()
    o1.setNchunks 16
    o1.setRedundancy 0
    o1.setFpathDb dir
    o1.setFpathChunks dir
    o1.setCompression true
    o1.setDeduplication 2

    Logging.enable_console ()

#if compile_for_windows
    let qfname = @"C:\Windows\notepad.exe"
#else
    let qfname = "/usr/bin/zip"
#endif

    (* create random file *)
    let fname = "./obj/random.output.tst"
    begin
        if File.Exists(fname) then
            File.Delete(fname)
        let inbytes = File.ReadAllBytes(qfname)
        use fstr = File.Open(fname, FileMode.OpenOrCreate, FileAccess.Write)
        fstr.Write(inbytes, 0, inbytes.Length)
        fstr.Flush()
        fstr.Close()
    end


    let b1 = BackupCtrl.create o1
    BackupCtrl.backup b1 fname
    BackupCtrl.finalize b1

    let bi1 = BackupCtrl.bytes_in b1
    let bo1 = BackupCtrl.bytes_out b1
    Assert.Greater(bi1, 0)
    Assert.Greater(bo1, 0)
    Logging.log () <| Printf.sprintf "1st run: we have been reading %d bytes and writing %d bytes\ncompression rate: %.2f%%" bi1 bo1 (100.0 * (1.0 - double(bo1)/double(bi1)))

(*    use tw1 = new StreamWriter( System.Console.OpenStandardOutput() )
    (BackupCtrl.getDbFp b1).outStream tw1
    (BackupCtrl.getDbKeys b1).outStream tw1
    tw1.Flush() *)

    (* new backup with previous backup db set as reference *)
    begin
        if File.Exists(fname) then
            File.Delete(fname)
        let inbytes = File.ReadAllBytes(qfname)
        use fstr = File.Open(fname, FileMode.OpenOrCreate, FileAccess.Write)
        fstr.Write(inbytes, 0, inbytes.Length)
        fstr.Flush()
        for i in [1..99] do
            for j in [1..99] do
                fstr.Write([|65uy;66uy;67uy;68uy;69uy;70uy;71uy;72uy;73uy|],0,9)
        fstr.Flush()
        fstr.Close()
    end

    let b2 = BackupCtrl.create o1
    BackupCtrl.setReference b2 (Some (BackupCtrl.getDbKeys b1)) (Some (BackupCtrl.getDbFp b1))
    BackupCtrl.backup b2 fname
    BackupCtrl.finalize b2

    let bi2 = BackupCtrl.bytes_in b2
    let bo2 = BackupCtrl.bytes_out b2
    Assert.Greater(bi2, 0)
    Assert.Greater(bo2, 0)
    Printf.printfn "2nd run: we have been reading %d bytes and writing %d bytes\ncompression rate: %.2f%%" bi2 bo2 (100.0 * (1.0 - double(bo2)/double(bi2)))

    use tw2 = new StreamWriter( System.Console.OpenStandardOutput() )
    (BackupCtrl.getDbFp b2).outStream tw2
    (BackupCtrl.getDbKeys b2).outStream tw2
    tw2.Flush()

    let test1 = match (BackupCtrl.getDbFp b1).idb.get fname with
                | None -> raise <| System.Exception "very bad a!"
                | Some (e : DbFpDat) ->
                    List.sortBy (fun b -> b.idx) e.blocks
                    |> List.fold (fun acc b -> acc + (Printf.sprintf "%d=%s" b.idx (Key256.toHex b.aid))) ""
    let test2 = match (BackupCtrl.getDbFp b2).idb.get fname with
                | None -> raise <| System.Exception "very bad b!"
                | Some (e : DbFpDat) ->
                    List.sortBy (fun b -> b.idx) e.blocks
                    |> List.fold (fun acc b -> acc + (Printf.sprintf "%d=%s" b.idx (Key256.toHex b.aid))) ""

    Assert.AreNotEqual(test1,test2)
