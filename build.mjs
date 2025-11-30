import * as esbuild from "esbuild";

const isWatch = process.argv.includes("--watch") || process.argv.includes("-w");

const ctx = await esbuild.context({
  entryPoints: ["src/app.mjs"],
  bundle: true,
  minify: false,
  sourcemap: false,
  target: ["es2020"],
  format: "iife",
  banner: {
    js: `
       function require(m) {
         const MODS = {
          "@microsoft/msfs-sdk": window.msfssdk,
          "@microsoft/msfs-garminsdk": window.garminsdk,
          "@microsoft/msfs-wtg3000-common": window.wtg3000common,
          "@microsoft/msfs-wtg3000-gtc": window.wtg3000gtc,
         }
        if(MODS[m])
          return MODS[m];
         throw new Error(\`Unknown module \${m}\`);
       }
    `,
  },

  jsx: "transform",
  jsxFactory: "msfssdk.FSComponent.buildComponent",
  jsxFragment: "msfssdk.FSComponent.Fragment",
  outfile:
    "./g3000-acars/PackageSources/Copys/garmin-3000-acars/plugin/garmin-3000-acars/plugin.js",
  external: [
    "@microsoft/msfs-sdk",
    "@microsoft/msfs-garminsdk",
    "@microsoft/msfs-wtg3000-common",
    "@microsoft/msfs-wtg3000-gtc",
  ],
});

if (isWatch) {
  console.log("Watching for changes...");
  await ctx.watch();
} else {
  await ctx.rebuild();
  await ctx.dispose();
  console.log("Build completed!");
}
