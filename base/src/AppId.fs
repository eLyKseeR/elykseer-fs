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

module internal AppId =

    (** this is an individual random key *)
    let appid = "d1e75ce87730af78f59618b75031e592a2c0530fea018420c515e4f51cf1627a"

    (** the salt is to select individual encryption with AES
        attention! data encrypted with one salt cannot be decrypted with another
     *)
    let salt = Key256.fromHex "a7261fc15f4e515c024810aef0350c2a295e13057b81695f87fa03778ec57e1d"
