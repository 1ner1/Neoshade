
#version 330 core
out vec4 OutColor;
in vec2 texCoords;
uniform sampler2D FBTex;
uniform sampler2D PrevFBTex;
uniform float randomizer;
uniform vec2 resolution;



// Options for presets
// [[BEGIN]]
vec3 tint = vec3(-0.15, -0.12, 0); // ambient color
bool filmGrain = false; // add slight noise for an authentic feel
bool chromaticAberration = false; // adds bending of light as a simulaton of a real life lens
bool vignette = true; // darkens corner of the screen
bool motionBlur = true; // blurs fast moving objects / little shaky good for car games
bool lightStreaks = true; // adds slight strikes to lights to make them more dramatic
bool improveLighting = true;

// [[END]]

// Ready effects
bool cartoonizeEffect = false;

// Other fine tunable settings

float NOISE_MOIRRE = 200.0; // higher noise moirre provide more analog tape ish look
float NOISE_BLOCKS = 1.0; // higher noise blocks provide more "digital noise" look than "analog noise look"
float NOISE_RATIO = 0.05;
float vignettePower = 0.70;

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

float luminanceCalc(vec3 color) {
  float luminance = (0.2126 * pow(color.x, 2.2)) + (0.7152  * pow(color.x, 2.2) + (0.0722 * pow(color.z, 2.2)));
  return luminance;
}

// to be implemented
vec3 directionalBlur(sampler2D textureIn, vec2 coords, vec2 direction) {
  return vec3(0.0);
}

vec3 improvedLighting(sampler2D textureIn, vec3 previousColor, vec2 coords) {
  vec3 outputColor = previousColor;
  for(int i = 0; i < 20; i++) {
    vec3 tempColor = vec3(texture2D(textureIn, vec2(coords.x + (cos(i * 1.0) / 50.0), coords.y + (sin(i * 1.0) / 50.0))));
    float luminance = luminanceCalc(tempColor);
    float threshold = 0.8;
    if(luminance > 0.8) {
      outputColor += tempColor / 10;
    }
  }

  return outputColor;
}

vec3 lightStreaksEffect(sampler2D textureIn, vec3 previousColor, vec2 coords) {
  vec3 outputColor = previousColor;
  for(int i = 0; i < 20; i++) {
    vec3 tempColor = vec3(texture2D(textureIn, vec2(coords.x + (cos(i * 1.0) / 20.0), coords.y + (sin(i * 1.0) / 20.0))));
    float luminance = luminanceCalc(tempColor);
    float threshold = 0.8;
    if(luminance > 0.8) {
      outputColor += tempColor / 10;
    }
  }

  return outputColor;
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
     // comparing difference between top left pixel for motion blur, kinda hacky but should work for most cases
     vec3 pixelC = vec3(texture2D(FBTex, texCoords / 100.0));
     vec3 pixelP = vec3(texture2D(PrevFBTex, texCoords / 100.0));
     float dist = distance(pixelC, pixelP);
     textureColor = mix(currentFB, boxBlur(PrevFBTex, texCoords, 16), dist);

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
      vec3 redCoords = mix(currentFB, vec3(boxBlur(PrevFBTex, texCoords / 1.02, 2)), 0.50);
      vec3 blueCoords = mix(currentFB, vec3(boxBlur(PrevFBTex, texCoords / 1.009, 2)), 0.50);

      endPass.x = mix(endPass.x, redCoords.x, 0.5);
      endPass.z = mix(endPass.z, blueCoords.z, 0.5);

    }        
    
    /*if(lightStreaks) {
      endPass = lightStreaksEffect(FBTex, endPass, texCoords);
    }*/

    if(improveLighting) {
      endPass = improvedLighting(FBTex, endPass, texCoords);
    }

    if(vignette) {
      float vignette = 1 - distance(gl_FragCoord.xy / resolution, vec2(0.5)) * vignettePower;
      endPass *= vec3(vignette);
    }

    // [[FILTERS_END]]

    OutColor = vec4(endPass, 0.0);
} 