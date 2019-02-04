using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Parser;
using CppSharp.Passes;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HalcyonPhysGenerator {
	public class HalcyonPhysXGenerator : ILibrary {
		public void Setup(Driver driver) {
			var pwdPath = Directory.GetCurrentDirectory();
			var binPath = Path.GetFullPath(Path.Combine(pwdPath, "../bin"));
			var physxIncludePath = Path.GetFullPath(Path.Combine(pwdPath, "../libsrc/physx/physx/include"));
			var pxsharedIncludePath = Path.GetFullPath(Path.Combine(pwdPath, "../libsrc/physx/pxshared/include"));
			var physxLibPath = Path.GetFullPath(Path.Combine(pwdPath, "../libsrc/physx/physx/bin/linux.clang/checked"));

			var options = driver.Options;
			//options.CheckSymbols = true; // Verify that the symobls exist. Seems there are issues with it.
			options.CommentKind = CommentKind.BCPLSlash;
			options.CompileCode = true; // Attempts to compile managed DLLs of the modules.
			//options.GenerateClassTemplates = true; // Causes the generator to crash with an out of bounds exception.
			options.GenerateDefaultValuesForArguments = true;
			options.GeneratorKind = GeneratorKind.CSharp;
			options.MarshalCharAsManagedChar = true;
			options.OutputDir = binPath;
			//options.GenerateDebugOutput = true; // Embeds the C++ source in the generated file for ready comparison.
			//options.GenerateSingleCSharpFile = false; // If false generates a .cs for every header, in addition to the main module .cs files...

			var dynamicLibExt = Platform.IsWindows ? ".dll" : (Platform.IsMacOS ? ".dylib" : ".so");
			var staticLibExt = Platform.IsWindows ? ".lib" : ".a";
			var libPaths = Directory
				.GetFiles(physxLibPath, "*" + staticLibExt, SearchOption.TopDirectoryOnly)
				.Concat(Directory.GetFiles(physxLibPath, "*" + dynamicLibExt, SearchOption.TopDirectoryOnly))
			;

			foreach (var libPath in libPaths) {
				var moduleName = Path.GetFileNameWithoutExtension(libPath);
				moduleName = moduleName.Substring(0, moduleName.IndexOf("_", StringComparison.InvariantCulture));
				if (moduleName.StartsWith("lib", StringComparison.InvariantCulture)) {
					moduleName = moduleName.Substring(3);
				}

				Console.WriteLine($"Reviewing {moduleName}...");
				if (!moduleName.StartsWith("physx", StringComparison.InvariantCultureIgnoreCase)) {
					Console.WriteLine($"Skipping {moduleName} as it is not a PhysX module...");
					continue;
				}

				var inclFolderName = moduleName.Substring("Physx".Length).ToLowerInvariant();
				if (inclFolderName == "pvdsdk") {
					inclFolderName = "pvd";
				}
				var inclFolderPath = Path.Combine(physxIncludePath, inclFolderName); // In the case of the PhysX module this will result in appending nothing.

				var headers = Directory.GetFiles(inclFolderPath, "*.h", SearchOption.AllDirectories);

				var module = options.AddModule(moduleName);

				module.IncludeDirs.Add(inclFolderPath);
				module.IncludeDirs.Add(pxsharedIncludePath);
				module.LibraryDirs.Add(physxLibPath);

				module.Headers.AddRange(headers);
				module.Headers.Remove("PxPhysicsAPI.h"); // We don't want the kitchen sink header.

				module.Defines.Clear();
				module.Defines.Add("_DEBUG");
			}
		}

		public void SetupPasses(Driver driver) {

		}

		public void Preprocess(Driver driver, ASTContext ctx) {
			foreach (var unit in ctx.TranslationUnits.Where(u => u.IsValid)) {
				IgnorePrivateDeclarations(unit);
			}

		}

		public void Postprocess(Driver driver, ASTContext ctx) {
		}

		private static void IgnorePrivateDeclarations(DeclarationContext unit) {
			foreach (var declaration in unit.Declarations) {
				IgnorePrivateDeclaration(declaration);
			}
		}

		private static void IgnorePrivateDeclaration(Declaration declaration) {
			if (declaration.Name != null &&
			(declaration.Name.StartsWith("Private", StringComparison.Ordinal) ||
			declaration.Name.EndsWith("Private", StringComparison.Ordinal))) {
				declaration.ExplicitlyIgnore();
			}
			else {
				var declarationContext = declaration as DeclarationContext;
				if (declarationContext != null) {
					IgnorePrivateDeclarations(declarationContext);
				}
			}
		}

		static class Program {
			public static void Main(string[] args) {
				ConsoleDriver.Run(new HalcyonPhysXGenerator());
				Console.WriteLine("Done");
			}
		}
	}
}
