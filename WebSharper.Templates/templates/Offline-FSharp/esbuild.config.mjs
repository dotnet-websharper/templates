import { readdirSync, cpSync } from 'fs'
import { build } from 'esbuild'

cpSync('./build/', './wwwroot/', { recursive: true, filter: source => source.endsWith('build') || source.endsWith('.html') });

const files = readdirSync('./build/Scripts/WebSharper/WebSharper.Offline.FSharp/');

files.forEach(file => {
  if (file.endsWith('.js')) {
    var options =
    {
      entryPoints: ['./build/Scripts/WebSharper/WebSharper.Offline.FSharp/' + file],
      bundle: true,
      minify: true,
      format: 'iife',
      outfile: 'wwwroot/Scripts/WebSharper/' + file,
      globalName: 'wsbundle'
    };

    console.log("Bundling:", file);
    build(options);
  }
});
