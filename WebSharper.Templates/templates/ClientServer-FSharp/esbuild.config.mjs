import { readdirSync, cpSync } from 'fs'
import { build } from 'esbuild'

cpSync('./build/Content/WebSharper/', './wwwroot/Content/WebSharper/', { recursive: true });

const files = readdirSync('./build/Scripts/WebSharper/WebSharper.ClientServer.FSharp/');

files.forEach(file => {
  if (file.endsWith('.js')) {
    var options =
    {
      entryPoints: ['./build/Scripts/WebSharper/WebSharper.ClientServer.FSharp/' + file],
      bundle: true,
      minify: true,
      format: 'iife',
      outfile: 'wwwroot/Scripts/WebSharper/' + file,
      globalName: 'wsbundle'
    };

    build(options);
  }
});
