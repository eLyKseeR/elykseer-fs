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

module Key128 =

    type CKey128 = lxr.SWIGTYPE_p_CKey128

    type t = { 
        //key128 : byte array;
        key128 : CKey128  // internal
    }

    //[<Literal>]
    let length = 128

    let nbytes = length / 8

    let ctype k = k.key128

    let create () = 
        { key128 = lxr.Key128.mk_Key128() }
        //let k = Key.create nbytes in
        //{ key128 = k }

    let toHex k = lxr.Key128.tohex_Key128(k.key128)
        //Key.toHex nbytes k.key128

    let fromHex s = 
        { key128 = lxr.Key128.fromhex_Key128(s) }
        //{ key128 = Key.fromHex nbytes s }

    let bytes k = lxr.Key128.tohex_Key128(k.key128) |>
                  Key.fromHex nbytes
