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

namespace Fsh

open System
open Xwt
open LXRgui

module FshButton =

(*    let create (lbl : string) =
        let b = new Xwt.Button(lbl)
        b.Font <- Xwt.Drawing.Font.SystemFont.WithSize(16.0).WithWeight(Drawing.FontWeight.Bold)
        b
*)

    let createWithHandler (lbl : string) (hdlr : Xwt.Button -> EventArgs -> unit) =
        let b = new Xwt.Button(lbl)
        //Console.WriteLine("new button with label '{0}'", lbl)
        //b.Font <- Xwt.Drawing.Font.SystemFont.WithSize(16.0).WithWeight(Drawing.FontWeight.Bold)
        //b.BackgroundColor <- Coloring.ink
        b.Clicked.Add(hdlr b)
        b

    let imageWithHandler (lbl : string) (fname : string) (hdlr : Xwt.Button -> EventArgs -> unit) =
        //Console.WriteLine("loading image {0}", fname)
        let img = Xwt.Drawing.Image.FromResource(fname)
        //Console.WriteLine("new button with image {0}", img)
        let b = new Xwt.Button(img (*, lbl*))
        b.Clicked.Add(hdlr b)
        b
