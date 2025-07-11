﻿import { cpSync, readdirSync, existsSync } from 'fs'
import { build } from 'esbuild'

cpSync('./build/', './wwwroot/', { recursive: true });

const prebundles = readdirSync('./build/Scripts/WebSharper/WebSharper.ClientServer.FSharp/');

prebundles.forEach(file => {
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

    console.log("Bundling:", file);
    build(options);
  }
});

if (existsSync('./build/Scripts/WebSharper/workers/')) {
  const workers = readdirSync('./build/Scripts/WebSharper/workers/');

  workers.forEach(file => {
    if (file.endsWith('.js')) {
      var options =
      {
        entryPoints: ['./build/Scripts/WebSharper/workers/' + file],
        bundle: true,
        minify: true,
        format: 'iife',
        outfile: 'wwwroot/Scripts/WebSharper/workers/' + file,
      };

      console.log("Bundling worker:", file);
      build(options);
    }
  });
}