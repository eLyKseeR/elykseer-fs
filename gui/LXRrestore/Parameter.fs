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

namespace LXRrestore
open System

module Parameter = 

    let regclass = "HKEY_CURRENT_USER"
    let company = "com.elykseer"
    let product = "LXRrestore"
    let version = "2"

    let key = regclass + "\\" + company + "\\" + product + "\\" + version

    let paramOrDefault n d = 
        Microsoft.Win32.Registry.GetValue(key, n, d)

    let setParameter n v = 
        Microsoft.Win32.Registry.SetValue(key, n, v)

    let stringOrDefault n d = 
        match paramOrDefault n d with
            | :? string as s -> s
            | _ -> ""

    let intOrDefault n d = 
        match paramOrDefault n d with
            | :? int as i -> i
            | _ -> 0
