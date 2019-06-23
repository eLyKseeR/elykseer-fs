with import <nixpkgs> {};

stdenv.mkDerivation rec {
    name = "env";

    #src = ./.;

    # Customizable development requirements
    nativeBuildInputs = [
        cmake
        git
        mono5
        fsharp
    ];

    buildInputs = [
        openssl
        zlib
    ];

}

