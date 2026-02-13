#ifndef SHADER_TYPES
#define SHADER_TYPES

struct FluidMediumProfile{
	float collisionDamping;
	float targetDensity;
	float pressureMultiplier;
	float nearPressureMultiplier;
	float viscosityStrength;
	float3 padding;
};

#endif