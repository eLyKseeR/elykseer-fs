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

type Restore () =

    let mutable n = 256
    let mutable r = 0
    let mutable pX = "/tmp/"
    let mutable pOut = "/tmp/"
    let mutable ctrl = eLyKseeR.RestoreCtrl.create ()
    let mutable selexpr = []
    let mutable xclexpr = []

    let init () =
        let o = new eLyKseeR.Options()
        o.setNchunks n
        o.setRedundancy r
        o.setFpathDb ""
        o.setFpathChunks pX
        eLyKseeR.RestoreCtrl.setOptions ctrl o
         
    let readfpdb fp =
        try
            (* read db file *)
            use str = new IO.FileStream(fp, IO.FileMode.Open)
            let db = eLyKseeR.RestoreCtrl.getDbFp ctrl
            db.inStream (new StreamReader(str))
            Console.WriteLine("know of {0} filepaths in db", db.idb.count)
            with _ -> ()
        ()
    let readkeydb fp =
        try
            (* read db file *)
            use str = new IO.FileStream(fp, IO.FileMode.Open)
            let db = eLyKseeR.RestoreCtrl.getDbKeys ctrl
            db.inStream (new StreamReader(str))
            Console.WriteLine("know of {0} keys in db", db.idb.count)
            with _ -> ()
        ()

    let dorestore (fp' : string) =
//#if compile_for_windows
//        let fpout = fp'.Replace(":", ",drive")
//#else
//        let fpout = fp'
//#endif
        let fpout = eLyKseeR.native.FsUtils.cleanfp fp'
        Console.Write("restoring "); gocyan()
        Console.Write(fp' + "  "); gonormal()
        try eLyKseeR.RestoreCtrl.restore ctrl pOut fp'
            let dbfp = eLyKseeR.RestoreCtrl.getDbFp ctrl
            match dbfp.idb.get fp' with
            | None -> gored(); Console.WriteLine("no data"); gonormal()
            | Some db ->
                let chk = eLyKseeR.Sha256.hash_file (pOut + "/" + fpout) |> eLyKseeR.Key256.toHex
                if chk = eLyKseeR.Key256.toHex db.checksum then
                    gogreen()
                    Console.WriteLine("success."); gonormal()
                else
                    gored()
                    Console.WriteLine("failure " + chk + "/=" + (eLyKseeR.Key256.toHex db.checksum)); gonormal()
        with
        | e -> gored(); Console.Write("failed"); gonormal()
               //Console.WriteLine(" with {0}", e.ToString())
               reraise ()

    let checkRestore (fp : string) =
        // check if in any selection regexp
        if List.exists (fun (pat : Regex) -> pat.IsMatch(fp)) selexpr then 

            // check if none in exclude regexp matches
            if List.forall (fun (pat : Regex) -> pat.IsMatch(fp) |> not) xclexpr then

                dorestore fp

        ()

    member this.setChunkPath p =
        pX <- p
        init ()

    member this.setOutputPath p =
        pOut <- p
        if not <| eLyKseeR.FileCtrl.dirExists p then
            Directory.CreateDirectory(p) |> ignore

    member this.setSelectExpr (x : string) =
        selexpr <- new Regex(x,RegexOptions.CultureInvariant) :: selexpr
        ()

    member this.setExcludeExpr (x : string) =
        xclexpr <- new Regex(x,RegexOptions.CultureInvariant) :: xclexpr
        ()


    member this.addDb fp =
        if eLyKseeR.FileCtrl.fileExists fp then
            readfpdb fp
            readkeydb fp
        ()

    member this.restore fp =
        dorestore fp

    member this.restoreSelection () =
        let db = eLyKseeR.RestoreCtrl.getDbFp ctrl
        // all paths are checked against regexps and eventually restored
        db.idb.appKeys (fun e -> checkRestore e)

    member this.summarize =
        let td = eLyKseeR.RestoreCtrl.time_read ctrl +
                 eLyKseeR.RestoreCtrl.time_decrypt ctrl +
                 eLyKseeR.RestoreCtrl.time_extract ctrl
        let bi = eLyKseeR.RestoreCtrl.bytes_in ctrl
        let bo = eLyKseeR.RestoreCtrl.bytes_out ctrl
        Console.WriteLine("restored {0:0,0} bytes (read {1:0,0} bytes); took read={2} ms decrypt={3} ms extract={4} ms",
            bo, bi,
            eLyKseeR.RestoreCtrl.time_read ctrl, eLyKseeR.RestoreCtrl.time_decrypt ctrl, eLyKseeR.RestoreCtrl.time_extract ctrl)
        Console.Write("compression rate: ")
        gocyan(); Console.Write("{0:0.00}", (double(bo) / double(bi)))
        gonormal(); Console.Write("  time: ")
        gocyan(); Console.Write("{0}", td)
        gonormal(); Console.Write(" ms  throughput: ")
        gocyan(); Console.Write("{0:0,0}", (double(bo) * 1000.0 / 1024.0 / double(td)))
        gonormal(); Console.WriteLine(" kilobytes per second")
        ()
