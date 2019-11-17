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

    exception BadLength
    exception ProcError of string

    let blockenc p (maxsz : uint32) (inbuf : byte[]) : byte[] =
        let len = uint32(Array.length inbuf)
        if len = uint32 0
        then Array.empty
        else
            let outbuf = Array.create (int(len)) (byte 0)
            let mutable tempb = Array.create (int(maxsz)) (byte 0)
            let mutable outlen = uint32 0
            let mutable count = uint32 0
            while count < len do
                let sz = min maxsz (len - count)
                let c = int(count)
                //System.Console.WriteLine(Printf.sprintf "sz = %i  count = %i" sz c)
                System.Buffer.BlockCopy(inbuf, c, tempb, 0, int(sz))
                let nproc = lxr.Aes.proc_AesEncrypt(p, uint32(sz), &tempb)
                count <- count + uint32(nproc)
                let available = lxr.Aes.len_AesEncrypt(p)
                let copied = lxr.Aes.copy_AesEncrypt(p, maxsz, &tempb)
                System.Buffer.BlockCopy(tempb, 0, outbuf, int(outlen), int(copied))
                outlen <- outlen + copied
                //System.Console.WriteLine(Printf.sprintf "nproc = %i  avail = %i  copied = %i tempb[0]='%02x' tempb[1]='%02x'"
                                                         //nproc available copied tempb.[0] tempb.[1])

            let fin = lxr.Aes.fin_AesEncrypt(p)
            if (fin > 0)
            then
                let copied = lxr.Aes.copy_AesEncrypt(p, maxsz, &tempb)
                System.Buffer.BlockCopy(tempb, 0, outbuf, int(outlen), int(copied))
            else ()
            outbuf

    let blockdec p (maxsz : uint32) (inbuf : byte[]) : byte[] =
        let len = uint32(Array.length inbuf)
        let outbuf = Array.create (int(len)) (byte 0)
        let mutable tempb = Array.create (int(maxsz)) (byte 0)
        let mutable outlen = uint32 0
        let mutable count = uint32 0
        while count < len do
            let sz = min maxsz (len - count)
            let c = int(count)
            let b = inbuf.[c .. c + int(sz) - 1]
            let nproc = lxr.Aes.proc_AesDecrypt(p, uint32(sz), ref b)
            count <- count + uint32(nproc)
            let available = lxr.Aes.len_AesDecrypt(p)
            let copied = lxr.Aes.copy_AesDecrypt(p, maxsz, &tempb)
            System.Buffer.BlockCopy(tempb, 0, outbuf, int(outlen), int(copied))
            outlen <- outlen + copied

        let fin = lxr.Aes.fin_AesDecrypt(p)
        if (fin > 0)
        then
            let copied = lxr.Aes.copy_AesDecrypt(p, maxsz, &tempb)
            System.Buffer.BlockCopy(tempb, 0, outbuf, int(outlen), int(copied))
        else ()
        outbuf

    let encrypt (key : Key256.t) (buf : byte array) : byte array =
        let iv = lxr.Key128.fromhex_Key128(AppId.salt)
        let aesEnc = lxr.Aes.mk_AesEncrypt(Key256.ctype key, iv)
        let encrypted = blockenc aesEnc (lxr.Aes.sz_AesEncrypt()) buf
        lxr.Key128.release_Key128(iv)
        lxr.Aes.release_AesEncrypt(aesEnc)
        encrypted

    let decrypt (key : Key256.t) (buf : byte array) : byte array =
        let iv = lxr.Key128.fromhex_Key128(AppId.salt)
        let aesDec = lxr.Aes.mk_AesDecrypt(Key256.ctype key, iv)
        let decrypted = blockdec aesDec (lxr.Aes.sz_AesDecrypt()) buf
        lxr.Key128.release_Key128(iv)
        lxr.Aes.release_AesDecrypt(aesDec)
        decrypted
