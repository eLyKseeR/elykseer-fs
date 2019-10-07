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

namespace eLyKseeR

module Aes =

    //open OpenSSL.Crypto

    exception BadLength

    let aes_crypt b l key n enc = 
        //let kbytes = Key256.bytes key
        //let abytes = Key256.bytes <| AppId.salt
        // salt is 8 bytes
        //let salt = Array.create 8 (byte 0)
        //for i = 0 to 7 do
            //salt.[i] <- abytes.[31-i]
        // iv is 128 bits (16 bytes)
        //let iv = Array.create 16 (byte 0)

        //let cc = new CipherContext(Cipher.AES_256_CBC)
        //let key = cc.BytesToKey(MessageDigest.SHA256, salt, kbytes, 1, ref iv)
        if enc then
            //cc.Encrypt(b, key, iv)
            let iv = lxr.Key128.fromhex_Key128(AppId.salt)
            let AesEnc = lxr.Aes.mk_AesEncrypt(Key256.ctype key, iv)
            let buf = System.Text.Encoding.UTF8.GetString(b)
            let nproc = lxr.Aes.proc_AesEncrypt(AesEnc, l, buf)
            lxr.Key128.release_Key128(iv)
            System.Text.Encoding.UTF8.GetBytes(buf.Substring(0,nproc))
        else
            //cc.Decrypt(b, key, iv)
            let iv = lxr.Key128.fromhex_Key128(AppId.salt)
            let AesDec = lxr.Aes.mk_AesDecrypt(Key256.ctype key, iv)
            let buf = System.Text.Encoding.UTF8.GetString(b)
            let nproc = lxr.Aes.proc_AesDecrypt(AesDec, l, buf)
            lxr.Key128.release_Key128(iv)
            System.Text.Encoding.UTF8.GetBytes(buf.Substring(0,nproc))

    let encrypt (k : Key256.t) (b : byte array) = 
        let l = Array.length b in
        if l % 16 <> 0 then raise BadLength
        else aes_crypt b l k 256 true

    let decrypt (k : Key256.t) (b : byte array) = 
        let l = Array.length b in
        if l % 16 <> 0 then raise BadLength
        else aes_crypt b l k 256 false
