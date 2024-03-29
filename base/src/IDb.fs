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

open System
open System.IO

type IDb<'k, 'e> =

    inherit IStreamIO

    abstract member contains : 'k -> bool
    abstract member get : 'k -> 'e option
    abstract member set : 'k -> 'e -> unit
    abstract member count : int
    abstract member union : IDb<'k,'e> -> unit
    abstract member appKeys : ('k -> unit) -> unit
    abstract member appValues : ('k -> 'e -> unit) -> unit

