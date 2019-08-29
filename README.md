[![CI build status](https://travis-ci.com/eLyKseeR/elykseer-fs.svg?branch=master)](https://travis-ci.com/eLyKseeR/elykseer-fs)

# elykseer-fs

This version of eLyKseeR builds on the PoC that was done in 2007 using F#: https://github.com/CodiePP/elykseer-base
Also included with this codebase are [command line utils](https://github.com/CodiePP/elykseer-cli)
and an experimental [GUI](https://github.com/CodiePP/elykseer-gui).


## hacking

the preferred way to inspect/hack/program eLyKseeR is via [nix-shell](https://nixos.org).

> `nix-shell`

## compilation

### key for signing

before compilation, prepare new RSA keys for signing assemblies:

> ``sn -k base/eLyKseeR.snk``  
> ``sn -k native/eLyKseeR-native.snk``  
> ``sn -k UT/ut.snk``  


### IDE support

> ``monodevelop eLyKseeR-base.Mono.sln &``

or open the [VisualStudio solution](eLyKseeR-base.Win32.sln) on Windows.


## submodules

run `git submodule update --remote` to get the latest versions

### [managed OpenSSL](https://github.com/CodiePP/openssl-net)

extract source code in the parent directory: ext/openssl-net.git
and build it.

### [sharpPRNG](https://github.com/CodiePP/prngsharp)

extract source code in the parent directory: ext/prngsharp.git
and build it. Also, run `mk_Linux.sh` to create the native library. For other
platforms call the appropriate script.
