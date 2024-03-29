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

namespace LXRcli

open System
open System.IO
open System.Text.RegularExpressions

open LXRcli.Coloring

type Backup () =

    let mutable n = 16
    let mutable r = 0
    let mutable pOut = "/tmp/"
    let mutable pDb = "/tmp/"
    //let mutable backup = None
    let mutable pCompress = false
    let mutable pDedup = 0
    let mutable refDbFp = None
    let mutable refDbKey = None
    let mutable paths : string list = []

//    let ctrl () =
//        match backup with
//        | Some c -> c
//        | None -> 
//            let o = new eLyKseeR.Options()
//            o.setNchunks n
//            o.setRedundancy r
//            o.setFpathDb pDb
//            o.setFpathChunks pOut
//            o.setCompression pCompress
//            o.setDeduplication pDedup
//            //Console.WriteLine ("initializing BackupCtrl...")
//            let c = eLyKseeR.BackupCtrl.create o
//            backup <- Some c
//            c
    
//    let trySetRefDb ctrl =
//        eLyKseeR.BackupCtrl.setReference ctrl refDbKey refDbFp

    member this.mkJob =
        let o = new eLyKseeR.Options()
        o.setNchunks n
        o.setRedundancy r
        o.setFpathDb pDb
        o.setFpathChunks pOut
        o.setCompression pCompress
        o.setDeduplication pDedup
       
        let job : eLyKseeR.DbJobDat = {
            options = o;
            regexincl = [new Regex(@"..*", RegexOptions.CultureInvariant)];
            regexexcl = [];
            paths = paths }
        job
      
    member this.runJob (name : string) (job : eLyKseeR.DbJobDat) =
        if not (eLyKseeR.FileCtrl.dirExists job.options.fpath_chunks) then
            Directory.CreateDirectory(job.options.fpath_chunks) |> ignore
        if not (eLyKseeR.FileCtrl.dirExists job.options.fpath_db) then
            Directory.CreateDirectory(job.options.fpath_db) |> ignore
        let ctrl = eLyKseeR.BackupCtrl.create job.options
        eLyKseeR.BackupCtrl.setReference ctrl refDbKey refDbFp
        gonormal()
        Console.Write("running job ")
        gocyan()
        Console.WriteLine(name)
        gonormal()

        eLyKseeR.Logging.enable_console ()
        Seq.iter (fun p -> eLyKseeR.BackupCtrl.backup ctrl p) job.paths
        eLyKseeR.BackupCtrl.finalize ctrl
        eLyKseeR.Logging.disable_console ()
        this.summarize ctrl

        gogreen()
        Console.WriteLine("done.")
        gonormal()
        Console.WriteLine("")

    member this.setN (x : string) =
        //Console.WriteLine("setting n = {0}", x)
        let k = try Int32.Parse x with
                | _ -> 16
        if k >= 16 && k <= 256 then
            n <- k

    member this.setR (x : string) =
        let k = try Int32.Parse x with
                | _ -> 0
        if k >= 0 && k <= 3 then
            r <- k

    member this.setC (x : string) =
        let k = try Int32.Parse x with
                | _ -> 0
        if k > 0 then
            pCompress <- true
        else
            pCompress <- false

    member this.setD (x : string) =
        let k = try Int32.Parse x with
                | _ -> 0
        if k >= 0 && k <= 2 then
            pDedup <- k

    member this.setChunkPath p =
        if eLyKseeR.FileCtrl.dirExists(p) then
            pOut <- p
        else
            if Directory.CreateDirectory(p).Exists then
                pOut <- p

    member this.setDataPath p =
        if eLyKseeR.FileCtrl.dirExists(p) then
            pDb <- p
        else
            if Directory.CreateDirectory(p).Exists then
                pDb <- p

    member this.setRefDbKey db =
        refDbKey <- Some db

    member this.setRefDbFp db =
        refDbFp <- Some db

//    member this.finalize =
//        eLyKseeR.BackupCtrl.finalize <| ctrl ()

    member this.backupFile fp =
        if eLyKseeR.FileCtrl.fileExists fp then
            (* backup file *)
            let fi = FileInfo(fp)
            if fi.Attributes.HasFlag(FileAttributes.ReparsePoint)
               || fi.Attributes.HasFlag(FileAttributes.System) then
                //gored ()
                eLyKseeR.Logging.log () <| Printf.sprintf "skipping: %A" fp
                //gonormal ()
            else
                //Console.Write("backing up ")
                //gocyan ()
                //Console.Write(fp)
                //gonormal ()
                paths <- fp :: paths
//                try eLyKseeR.BackupCtrl.backup ctrl fp
//                    gogreen(); Console.WriteLine(" done."); gonormal()
//                with
//                | _ -> gored(); Console.WriteLine(" failed."); gonormal()
        ()

    member this.backupDirectory fp =
        if eLyKseeR.FileCtrl.dirExists fp then
            (* backup directory *)
            let di = DirectoryInfo(fp)
            for fps in di.EnumerateFiles() do
                this.backupFile fps.FullName
        ()

    member this.backupRecursive fp =
        if eLyKseeR.FileCtrl.dirExists fp then
            (* backup directory *)
            let di = DirectoryInfo(fp)
            for fps in di.EnumerateFiles() do
                this.backupFile fps.FullName
            for dp in di.EnumerateDirectories() do
                this.backupRecursive dp.FullName
        ()

    member this.summarize ctrl =
        let td = eLyKseeR.BackupCtrl.time_encrypt ctrl +
                 eLyKseeR.BackupCtrl.time_extract ctrl +
                 eLyKseeR.BackupCtrl.time_write ctrl
        let bi = eLyKseeR.BackupCtrl.bytes_in ctrl
        let bo = eLyKseeR.BackupCtrl.bytes_out ctrl
        Console.WriteLine("backup {0:0,0} bytes (read {1:0,0} bytes); took write={2} ms encrypt={3} ms extract={4} ms",
            bo, bi,
            eLyKseeR.BackupCtrl.time_write ctrl, eLyKseeR.BackupCtrl.time_encrypt ctrl, eLyKseeR.BackupCtrl.time_extract ctrl)
        Console.Write("compression rate: ")
        gocyan ()
        Console.Write("{0:0.00}", (double(bi) / double(bo)))
        gonormal ()
        Console.Write("  time: ")
        gocyan ()
        Console.Write("{0}", td)
        gonormal ()
        Console.Write(" ms  throughput: ")
        gocyan ()
        Console.Write("{0:0,0}", (double(bi) * 1000.0 / 1024.0 / double(td)))
        gonormal ()
        Console.WriteLine(" kilobytes per second")
        ()
