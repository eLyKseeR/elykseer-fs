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

module TestAes

open System
open NUnit.Framework
open eLyKseeR

[<Test>]
let ``can encrypt and decrypt``() =
    let k = Key256.create ()
    let smsg = "this is a message to be en/decrypted and compared"
    let msg = Array.create (String.length smsg) (byte 0)
    String.iteri (fun i c -> msg.[i] <- byte(c)) smsg

    printf "\noriginal plaintext: \n"
    Array.iter (fun b -> printf "%02x " b) msg
    printf "\n"

    let msg2 = Aes.encrypt k msg
    printf "\nencrypted:  \n"
    Array.iter (fun b -> printf "%02x " b) msg2
    printf "\n"

    let msg3 = Aes.decrypt k msg2
    printf "\ndecrypted:  \n"
    Array.iter (fun b -> printf "%02x " b) msg3
    printf "\n"
    Assert.AreEqual(msg, msg3)
    Assert.AreEqual(smsg, msg3.[0 .. String.length smsg - 1])

    let msg4 = Aes.encrypt k msg3
    let msg5 = Aes.decrypt k msg4
    Assert.AreEqual(msg, msg5)

[<Test>]
let ``huge buffer: encrypt and decrypt``() =
    let k = Key256.create ()
    let smsg = "0123456789"
    let sz = 16*Chunk.width*Chunk.height // 4 MB
    let msg = Array.create sz (byte 0)

    let mutable tenc = Array.create 100 (int 0)
    let mutable tdec = Array.create 100 (int 0)

    for i in 1 .. 100 do
        let t0 = DateTime.Now
        let msg2 = Aes.encrypt k msg
        Assert.AreEqual(msg.Length, msg2.Length)

        let t1 = DateTime.Now
        let msg3 = Aes.decrypt k msg2
        let t2 = DateTime.Now
        tenc.[i-1] <- (t1 - t0).Milliseconds
        tdec.[i-1] <- (t2 - t1).Milliseconds

        Assert.AreEqual(msg.Length, msg3.Length)
        Assert.AreEqual(msg, msg3)

    //System.Console.WriteLine("time for encryption: {0} ms", (t1 - t0).Milliseconds)
    //System.Console.WriteLine("time for decryption: {0} ms", (t2 - t1).Milliseconds)
    System.Array.Sort(tenc)
    System.Array.Sort(tdec)
    System.Console.WriteLine("time for encryption: {0} < {1} < {2}", tenc.[0], tenc.[50], tenc.[99])
    System.Console.WriteLine("time for decryption: {0} < {1} < {2}", tdec.[0], tdec.[50], tdec.[99])