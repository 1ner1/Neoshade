
#version 330 core
out vec4 OutColor;
in vec2 texCoords;
uniform sampler2D FBTex;
uniform sampler2D PrevFBTex;
uniform float randomizer;
uniform vec2 resolution;

// source: https://github.com/Experience-Monks/glsl-fast-gaussian-blur
vec4 blur5(sampler2D image, vec2 uv, vec2 resolution, vec2 direction) {
  vec4 color = vec4(0.0);
  vec2 off1 = vec2(1.3333333333333333) * direction;
  color += texture2D(image, uv) * 0.29411764705882354;
  color += texture2D(image, uv + (off1 / resolution)) * 0.35294117647058826;
  color += texture2D(image, uv - (off1 / resolution)) * 0.35294117647058826;
  return color; 
}


// Options for presets
// [[BEGIN]]
vec3 tint = vec3(-0.15, -0.12, 0);
bool filmGrain = false;
bool chromaticAberration = false;
bool vignette = true;
bool motionBlur = true;
float vignettePower = 0.70;

// [[END]]

// Ready effects
bool cartoonizeEffect = false;

// Other fine tunable settings

float NOISE_MOIRRE = 20.0; // higher noise blocks provide more "digital noise" look than "analog noise look"
float NOISE_RATIO = 0.05;
highp float random(highp vec2 coords) {
   return fract(sin(dot(coords.xy, vec2(12.9898,78.233))) * 43758.5453);
}

vec3 boxBlur(sampler2D textureIn, vec2 coords, int area) {
  vec3 outputColor = vec3(1.0);
  for(int i = 0; i < area; i++) {
    outputColor += vec3(texture2D(textureIn, coords + (cos(i * 10.0) / 100.0)));
  }

  return vec3(outputColor.x / area, outputColor.y / area, outputColor.z / area);

  
}


vec3 cartoonize(vec3 texData, vec2 coords) {
  float luminance = cos(distance(0.5,(0.2126 * pow(texData.x, 2.2)) + (0.7152  * pow(texData.y, 2.2) + (0.0722 * pow(texData.z, 2.2)))));
  // cartoon dots
  float r = 1.0 ;
  float loc = 1.5 * r * (3.0 + (luminance * 5.0));
  coords = mod(coords, loc) - loc / 2.0;
  float dotPass = length(coords) - r;  

  vec3 returnColor = texData;
  returnColor.x *= dotPass;
  returnColor.y *= dotPass;
  returnColor.z *= dotPass;

  // posterize

  returnColor.x = floor(returnColor.x * 8) / 8;
  returnColor.y = floor(returnColor.y * 8) / 8;
  returnColor.z = floor(returnColor.z * 8) / 8;

  return returnColor;
}

void main()
{
    vec3 currentFB = vec3(texture2D(FBTex, texCoords));
    vec3 prevFB = vec3(texture2D(PrevFBTex, texCoords));
    vec3 textureColor = currentFB; 
    if(motionBlur) {
     textureColor = mix(currentFB, boxBlur(PrevFBTex, texCoords, 16), 0.10);

    }
    vec3 reflectionSSR = vec3(texture2D(FBTex, vec2(texCoords.x, 1 - texCoords.y)));
    float luminance = (0.2126 * pow(textureColor.r, 2.2)) + (0.7152  * pow(textureColor.g, 2.2) + (0.0722 * pow(textureColor.b, 2.2)));

    if((gl_FragCoord.y) >= resolution.y / 2) {
      reflectionSSR.r = textureColor.r;
      reflectionSSR.g = textureColor.g;
      reflectionSSR.b = textureColor.b;
    }

    // [AUTO GENERATED = PRESET BUILDER]


    
    // ssgi light transfer process
    

    // ssr luminance for weighted mixing, more bright objects should cast more vivid reflections
    float luminanceSSR = (0.2126 * pow(reflectionSSR.r, 2.2)) + (0.7152  * pow(reflectionSSR.g, 2.2) + (0.0722 * pow(reflectionSSR.b, 2.2)));
    vec3 ssrPass = mix(textureColor, reflectionSSR, 0.15 + ((luminanceSSR > 0.9 ? 1.0 : 0.0) * 1.0) );
    vec3 tintPass = ssrPass + tint;
    vec3 ditheringPass = tintPass;
    if(filmGrain) {
      ditheringPass += mix(-NOISE_RATIO, NOISE_RATIO, random((gl_FragCoord.xy / NOISE_MOIRRE) * randomizer));
    }



    
    
    vec3 endPass = ditheringPass;

    // [[FILTERS_BEGIN]]

    // CARTOON FILTER

    if(cartoonizeEffect == true) {
      endPass = cartoonize(tintPass, gl_FragCoord.xy);

    }

    if(chromaticAberration) {
      vec3 redCoords = mix(currentFB, vec3(blur5(PrevFBTex, texCoords / 1.02, vec2(400, 400), vec2(0.2, 0.2))), 0.50);
      vec3 blueCoords = mix(currentFB, vec3(blur5(PrevFBTex, texCoords / 1.009, vec2(400, 400), vec2(0.2, 0.2))), 0.50);

      endPass.x = mix(endPass.x, redCoords.x, 0.5);
      endPass.z = mix(blueCoords.x, blueCoords.z, 0.5);

    }        
    

    if(vignette) {
      float vignette = 1 - distance(gl_FragCoord.xy / resolution, vec2(0.5)) * vignettePower;
      endPass *= vec3(vignette);
    }

    // [[FILTERS_END]]

    OutColor = vec4(endPass, 0.0);
} 