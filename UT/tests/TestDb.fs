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

module TestDb

open System
open NUnit.Framework
open eLyKseeR
open System.IO
open System.Xml

[<Test>]
let ``filepath db``() =
    let fp = "/the/path/file.dat-1" in
    let db = new DbFp() in
    let b1 : DbFpBlock = {
        aid=Key256.fromHex "f9b30c7d12a58bfed5cee6f7ba8d1d3b8094ffed38da8a14be5af8a315e00812"
        idx = 1; apos=137; fpos=0L; blen=9847; clen=9847;
        compressed=false; checksum=Key128.fromHex "a58bfef9b30f9b30c7d12c7d12a58bfe" } in
    let d1 : DbFpDat = {
        id=Key128.fromHex "1234567890abcdef1234567890abcdef";
        len=9847L; osusr="nobody"; osgrp="users"; osattr="none";
        checksum=Key256.fromHex "3b80f9b30c7d16f7ba8d1d94ffee5af8a31eeb12008d38d5e142a58bfed5ca8a";
        blocks = [b1]; }
    Assert.AreEqual(0, db.idb.count)

    //Assert.Throws<System.Collections.Generic.KeyNotFoundException>( fun() -> DbFp.get db "/some/thing/not/seen/yet.txt" |> ignore ) |> ignore
    Assert.AreEqual(None, db.idb.get "/some/thing/not/seen/yet.txt")

    Directory.CreateDirectory "testing" |> ignore
    db.idb.set fp d1
    Assert.AreEqual(1, db.idb.count)
    Assert.AreEqual(Some d1, db.idb.get fp)
    use fstr = File.CreateText "./testing/test_dbfp.xml" in
    begin
        db.outStream fstr
        fstr.Flush()
        fstr.Close()
    end
    use instr = new StreamReader(File.OpenRead "./testing/test_dbfp.xml") in
        db.inStream instr
    // rereading the same data does not add anything
    Assert.AreEqual(1, db.idb.count)

[<Test>]
let ``key db``() =
    let aid1 = "94ffed38da8acee6f14be5af8a31d3b8015e008f9b30c7d12a58bfed57ba8d12" in
    let aid2 = "a31d3b8015e00894ffed38da8acee6f14be5af8f9b30c7d12a58bfed57ba8d12" in
    let db = new DbKey () in
    let d1 : DbKeyDat = { key=Key256.create (); iv=[|byte('f');byte('f')|]; n=32; } in
    let d2 : DbKeyDat = { key=Key256.create (); iv=[|byte('f');byte('e')|]; n=32; } in
    Assert.AreEqual(0, db.idb.count)
    //Assert.Throws<System.Collections.Generic.KeyNotFoundException>( fun() -> db.idb.get aid1 |> ignore ) |> ignore
    Assert.AreEqual(None, db.idb.get aid1)
    db.idb.set aid1 d1
    Assert.AreEqual(1, db.idb.count)
    Assert.AreEqual(Some d1, db.idb.get aid1)
    db.idb.set aid2 d2
    Assert.AreEqual(2, db.idb.count)
    Assert.AreEqual(Some d2, db.idb.get aid2)
    use fstr = File.CreateText "./testing/test_dbkey.xml" in
    begin
        db.outStream fstr
        fstr.Flush()
        fstr.Close()
    end
    use instr = new StreamReader(File.OpenRead "./testing/test_dbkey.xml") in
        db.inStream instr
    // rereading the same data does not add anything
    Assert.AreEqual(2, db.idb.count)

[<Test>]
let ``read key db from file``() =
    let db = new DbKey()
    let fname = "./testing/test_read_db_key.xml"
    File.Delete(fname)
    use fstr1 = File.OpenWrite(fname)
    use s = new StreamWriter(fstr1)
    s.Write("<?xml version=\"1.0\"?>\n")
    s.Write("<DbKey xmlns=\"http://spec.elykseer.com/lxr/v1.0\">\n")
    s.Write("<host>host</host>\n")
    s.Write("<user>user</user>\n")
    s.Write("<date>2017-12-31T22:11:33</date>\n")
    s.Write("  <Key aid=\"9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396\" n=\"16\" iv=\"7b584afa3156caed5c2e720c849ca852\">b498558df7fee3478421871e8182deda9ab518cc13c811b2b44f89d31379db12</Key>\n")
    s.Write("</DbKey>\n")
    s.Close()
    fstr1.Close()

    use fstr2 = File.OpenRead(fname)
    use instr = new StreamReader(fstr2)
    db.inStream instr
    Assert.AreEqual(1, db.idb.count)

[<Test>]
let ``read fp db from file``() =
    let db = new DbFp()
    let fname = "./testing/test_read_db_fp.xml"
    File.Delete(fname)
    use fstr1 = File.OpenWrite(fname)
    use s = new StreamWriter(fstr1)
    s.Write("<?xml version=\"1.0\"?>\n")
    s.Write("<DbFp xmlns=\"http://spec.elykseer.com/lxr/v1.0\">\n")
    s.Write("<host>host</host>\n")
    s.Write("<user>user</user>\n")
    s.Write("<date>2017-12-31T22:11:33</date>\n")
    //s.Write("""  <Fp fp="/bin/bash" len="1037528" osusr="ignored" osgrp="ignored" attr="ignored" chksum="c2615a71ff5c004e51aef248103a2950c25715f5eb8130837695770e1d78ecfa">""")
    s.Write("""  <Fp id="0123456789abcdef0123456789abcdef" fp="/bin/bash">""")
    s.Write("""    <Fattrs><osusr>ignored</osusr><osgrp>ignored</osgrp><length>1037528</length><last>ignored</last><chksum>c2615a71ff5c004e51aef248103a2950c25715f5eb8130837695770e1d78ecfa</chksum></Fattrs>""")
    s.Write("""    <Fblock idx="1" apos="18140" fpos="0" blen="65536" clen="29248" compressed="1" chksum="37b76e7b6b2ca218397d5b235b69b3ed">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="2" apos="47388" fpos="65536" blen="65536" clen="24847" compressed="1" chksum="2bed809ad6b2628c83e02b3b6de45269">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="3" apos="72235" fpos="131072" blen="65536" clen="36692" compressed="1" chksum="049754db4cad1bbb69204b850de008be">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="4" apos="108927" fpos="196608" blen="65536" clen="37521" compressed="1" chksum="442b09bd3e92a59b0eb7ab7402d81d5c">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="5" apos="146448" fpos="262144" blen="65536" clen="37107" compressed="1" chksum="6da9c0ff9a856d1d76733337cee1994b">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="6" apos="183555" fpos="327680" blen="65536" clen="37500" compressed="1" chksum="296d14f7906514b130c368d478a30629">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="7" apos="221055" fpos="393216" blen="65536" clen="37099" compressed="1" chksum="596e96ca23ad7971eca15d1151f7811c">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="8" apos="258154" fpos="458752" blen="65536" clen="37700" compressed="1" chksum="0fac69139db3f30821688758c981a26d">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="9" apos="295854" fpos="524288" blen="65536" clen="37634" compressed="1" chksum="e23aa591e03a1cc32dc731b54f6d065b">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="10" apos="333488" fpos="589824" blen="65536" clen="37554" compressed="1" chksum="3bd15e5024efe52e1e8a0ba63fc219b9">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="11" apos="371042" fpos="655360" blen="65536" clen="39404" compressed="1" chksum="1d55230a79d9c1877d2dfbfdc0f9f8d9">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="12" apos="410446" fpos="720896" blen="65536" clen="34423" compressed="1" chksum="8d0ba86afb98be06b9406cca22308841">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="13" apos="444869" fpos="786432" blen="65536" clen="19363" compressed="1" chksum="a8f68cfc2a9f441e1e024bc81ada2bfa">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="14" apos="464232" fpos="851968" blen="65536" clen="23742" compressed="1" chksum="a0a3f3e36a2cbd2777d5099b97d36809">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="15" apos="487974" fpos="917504" blen="65536" clen="22491" compressed="1" chksum="c08a6c59d941c41f8e7e0d8b65621237">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""    <Fblock idx="16" apos="510465" fpos="983040" blen="54488" clen="10812" compressed="1" chksum="94462811c262b02ed49e50137c00ad70">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""  </Fp>""")
    s.Write("""  <Fp id="abcdef0123456789abcdef0123456789" fp="/bin/more" len="39768" osusr="ignored" osgrp="ignored" attr="ignored" chksum="16c7432f3c72f4703b876b517f0154e9594bd835a358c81ec716b6719c35ace3">""")
    s.Write("""    <Fblock idx="1" apos="33" fpos="0" blen="39768" clen="18107" compressed="1" chksum="96621d99ad1b3eefaf5cb7fec1ec8293">9468e615055730a0152ed80dda6c459280bc8e7288bfb3cba9a736932e046396</Fblock>""")
    s.Write("""  </Fp>""")
    s.Write("""</DbFp>""")
    s.Close()
    fstr1.Close()

    use fstr2 = File.OpenRead(fname)
    use instr = new StreamReader(fstr2)
    db.inStream instr
    Assert.AreEqual(2, db.idb.count)
    let fp0 = db.idb.get "/bin/bash"
    match fp0 with
    | None -> ()
    | Some fp ->
        Assert.AreEqual(16, List.length fp.blocks)

[<Test>]
let ``write Options to file``() =
    let d = new Options()
    let fname = "./testing/test_write_options.xml"
    File.Delete(fname)
    use fstr1 = File.OpenWrite(fname)
    use s = new StreamWriter(fstr1)
    d.io.outStream(s)

[<Test>]
let ``read options from file``() =
    let d = new Options()
    let fname = "./testing/test_read_options.xml"
    File.Delete(fname)
    use fstr1 = File.OpenWrite(fname)
    use s = new StreamWriter(fstr1)
    s.Write("<Options>\n")
    s.Write("  <nchunks>253</nchunks>\n")
    s.Write("  <redundancy>0</redundancy>\n")
    s.Write("  <fpathchunks>/tmp/LXR</fpathchunks>\n")
    s.Write("  <fpathdb>/tmp/meta</fpathdb>\n")
    s.Write("  <compression>True</compression>\n")
    s.Write("  <deduplication>2</deduplication>\n")
    s.Write("</Options>\n")
    s.Close()
    fstr1.Close()
  
    use fstr2 = File.OpenRead(fname)
    //use instr = new StreamReader(fstr2)
    use reader = new XmlTextReader(fstr2)
    Console.WriteLine("going to read ..")
    d.io.inStream reader
    Console.WriteLine("comparing option ..")
    Assert.AreEqual(253, d.nchunks)
    Assert.AreEqual("/tmp/LXR", d.fpath_chunks)
    Assert.AreEqual("/tmp/meta", d.fpath_db)
    Assert.AreEqual(2, d.isDeduplicated)
    Assert.AreEqual(true, d.isCompressed)

[<Test>]
let ``write BackupJob to file``() =
    let d = new DbBackupJob()
    let fname = "./testing/test_write_backupjob.xml"
    File.Delete(fname)
    use fstr1 = File.OpenWrite(fname)
    use s = new StreamWriter(fstr1)
    d.outStream(s)

[<Test>]
let ``read BackupJob from file``() =
    let basedir = "/var/tmp/testbed"
    Directory.CreateDirectory basedir |> ignore
    File.WriteAllText(basedir + "/file.txt", "this is some text.")
    File.WriteAllText(basedir + "/file.jpg", "you would expect some image date here.")
    let d = new DbBackupJob()
    let p1 = @"C:\HOME\me\Documents"
    let p2 = @"testbed"
    let fname = "./testing/test_read_backupjob.xml"
    File.Delete(fname)
    use fstr1 = File.OpenWrite(fname)
    use s = new StreamWriter(fstr1)
    s.WriteLine("<DbBackupJob>")
    s.WriteLine(@"<Job name=""{0}"">", p1)
    s.WriteLine("<Paths>")
    s.WriteLine(@"<path type=""directory"">{0}</path>", p1)
    s.WriteLine(@"<path type=""file"">{0}</path>", p1)
    s.WriteLine("</Paths>")
    s.WriteLine("<Options>")
    s.WriteLine("  <nchunks>42</nchunks>")
    s.WriteLine("  <redundancy>0</redundancy>")
    s.WriteLine("  <fpathchunks>/tmp/LXR</fpathchunks>")
    s.WriteLine("  <fpathdb>/tmp/meta</fpathdb>")
    s.WriteLine("  <compression>True</compression>")
    s.WriteLine("  <deduplication>1</deduplication>")
    s.WriteLine("</Options>")
    s.WriteLine("</Job>")
    s.WriteLine(@"<Job name=""{0}"">", p2)
    s.WriteLine("<Paths>")
    s.WriteLine(@"<path type=""recursive"">{0}</path>", basedir)
    s.WriteLine("</Paths>")
    s.WriteLine("<Filters>")
    s.WriteLine(@"<exclude>.*\.jpeg</exclude><exclude>.*\.jpg</exclude>")
    s.WriteLine("<include>vacation*</include>")
    s.WriteLine("<include>..*</include>")
    s.WriteLine("</Filters>")
    s.WriteLine("<Options>")
    s.WriteLine("  <nchunks>16</nchunks>")
    s.WriteLine("  <redundancy>0</redundancy>")
    s.WriteLine("  <fpathchunks>/tmp/LXR</fpathchunks>")
    s.WriteLine("  <fpathdb>/tmp/meta</fpathdb>")
    s.WriteLine("  <compression>False</compression>")
    s.WriteLine("  <deduplication>2</deduplication>")
    s.WriteLine("</Options>")
    s.WriteLine("</Job>")
    s.WriteLine("</DbBackupJob>")
    s.Close()
    fstr1.Close()
  
    use fstr2 = File.OpenRead(fname)
    use instr = new StreamReader(fstr2)
    d.inStream instr
    Assert.AreEqual(2, d.idb.count)

    let e1 = match d.idb.get(p1) with
             | Some(e) -> e
             | _ -> {regexexcl=[]; regexincl=[]; options=new Options(); paths=[]}
    let e2 = match d.idb.get(p2) with
             | Some(e) -> e
             | _ -> {regexexcl=[]; regexincl=[]; options=new Options(); paths=[]}

    Console.WriteLine("entry 1 = {0}, {1}", e1.regexexcl, e1.regexincl)
    Console.WriteLine("entry 2 = {0}, {1}", e2.regexexcl, e2.regexincl)

    Assert.AreEqual(1, e1.options.isDeduplicated)
    Assert.AreEqual(2, e2.options.isDeduplicated)
    Assert.AreEqual(0, e1.paths.Length)
    Assert.AreEqual(2, e2.paths.Length)  // TODO should be only 1 (.jpg filtered out)

    let fname2 = "./testing/test_dump_backupjob.xml"
    File.Delete(fname2)
    use fstr3 = File.OpenWrite(fname2)
    use s2 = new StreamWriter(fstr3)
    d.outStream(s2)
