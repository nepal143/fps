## ATG URP and VegetationStudioPro

If you have installed the latest ATG_URP_10_Shaders or ATG_URP_11_Shaders package the URP shaders will support VegetationStudioPro.

# Requirements
- Unity 2020.1 or higher.
- URP 10.x or URP 11.x.
- VSP 1.4.5 (only version I have tested it with).
- ATG_URP_10_Shaders or ATG_URP_11_Shaders must be installed.

# Limitations
- Shaders do not support VSP’s touch react system but Lux SRP Grass Displacement.
- Supported shaders are grass and foliage. VertexLit is not supported as VSP has its own shaders for this.
- LOD cross fading is not supported.
- Texture arrays are not supported.

# Usage
- In order to make it all work, check "Enable VSP Support". This will make the shaders use the matrix buffers as provided by VSP's instancedindirect drawing.
- "VSP Scale Multiplier" ATG applies color variation based on scale. In case you use a scale multiplier in VSP settings you have to add it here to get proper color variation.
- "VSP Cull Distance" VSP has changed to fully rely on LODs which are not supported by ATG right now. So in order to fade out the instances over distance, set the cull distance here which should match the ditance at which VSP stops drawing the given prototype.
- "VSP Cull Fade" lets you specify the range over which the prototypes will fade out. Smaller values here increase the range.

