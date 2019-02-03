#!/bin/bash

BASE_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PHYSX_DIR="$BASE_DIR/libsrc/physx"
CPP_SHARP_DIR="$BASE_DIR/libsrc/cppsharp"
FINAL_OUTPUT_DIR="$BASE_DIR/bin"
LIB_OUTPUT_DIR="$BASE_DIR/lib"

if !(git --help > /dev/null 2>&1); then
  echo "Missing required tool!"
  echo "Please make sure git is installed and on the PATH."
  exit 1
fi

if !(mono -V > /dev/null 2>&1); then
  echo "Missing required tool!"
  echo "Please make sure mono is installed and on the PATH."
  exit 1
fi

mkdir -p "$PHYSX_DIR"
mkdir -p "$CPP_SHARP_DIR"
mkdir -p "$FINAL_OUTPUT_DIR"
mkdir -p "$LIB_OUTPUT_DIR/physx_include"

(
  cd "$PHYSX_DIR"
  if [[ -d "$PHYSX_DIR/.git" ]]; then
    echo "# Updating PhysX..."
    git pull
  else
    echo "# Fetching PhysX..."
    git clone https://github.com/NVIDIAGameWorks/PhysX.git "$PHYSX_DIR"
    git checkout "4.0"
  fi

  echo "# Building PhysX..."
  cd physx
  if ! ./generate_projects.sh linux; then
    exit 1
  fi

  # Using the checked edition to gain the error checking at the expense of speed.
  cd compiler/linux-checked
  if ! make -j$(( $(nproc) + 1 )); then
    exit 1
  fi

  echo "# PhysX ready."
) || exit 1

(
  cd "$CPP_SHARP_DIR"
  if [[ -d "$CPP_SHARP_DIR/.git" ]]; then
    echo "# Updating CppSharp..."
    git pull
  else
    echo "# Fetching CppSharp..."
    git clone https://github.com/mono/CppSharp.git "$CPP_SHARP_DIR"
    git checkout "master"
  fi

  cd build
  if ! compgen -G 'scripts/llvm-*' > /dev/null; then
    echo "# Fetching LLVM dependency..."
    if ! ( ./premake5-linux-64 --file=scripts/LLVM.lua download_llvm ); then
      exit 1
    fi
  fi
  echo "# Building CppSharp..."
  if ! ( ./Compile.sh ); then
    exit 1
  fi

  echo "# CppSharp ready."
) || exit 1

(
  echo "# Copying files around..."

  # Using the checked edition to gain the error checking at the expense of speed.
  if ! ( cp "$PHYSX_DIR/physx/bin/linux.clang/checked/libPhysXGpu_64.so" "$FINAL_OUTPUT_DIR/" ); then
    exit 1
  fi

  if ! ( cp -R "$CPP_SHARP_DIR/build/gmake/lib/Release_x64/"* "$LIB_OUTPUT_DIR/" ); then
    exit 1
  fi
) || exit 1

echo "# Done."
