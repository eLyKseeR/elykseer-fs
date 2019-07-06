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

module Coloring =

    open System

#if compile_for_windows
    let goblack () = Console.ForegroundColor <- ConsoleColor.Black
    let gored () = Console.ForegroundColor <- ConsoleColor.Red
    let gogreen () = Console.ForegroundColor <- ConsoleColor.Green
    let goyellow () = Console.ForegroundColor <- ConsoleColor.Yellow
    let goblue () = Console.ForegroundColor <- ConsoleColor.Blue
    let gomagenta () = Console.ForegroundColor <- ConsoleColor.Magenta
    let gocyan () = Console.ForegroundColor <- ConsoleColor.Cyan
    let gowhite () = Console.ForegroundColor <- ConsoleColor.White
    let normal0 = Console.ForegroundColor
    let gonormal () = Console.ForegroundColor <- normal0
#else
    let goblack () = Console.Write("\u001b[30m")
    let gored () = Console.Write("\u001b[31m")
    let gogreen () = Console.Write("\u001b[32m")
    let goyellow () = Console.Write("\u001b[33m")
    let goblue () = Console.Write("\u001b[34m")
    let gomagenta () = Console.Write("\u001b[35m")
    let gocyan () = Console.Write("\u001b[36m")
    let gowhite () = Console.Write("\u001b[37m")
    let gonormal () = Console.Write("\u001b[39m")
#endif

