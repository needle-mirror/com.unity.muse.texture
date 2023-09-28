---
uid: set-as-reference
---

# Generate from a reference image

You can generate textures guided by the color, shape, or pattern of a reference image.

## Import a reference image

A reference image can be an image in the **Generations** panel, in the **Project** window, in the **Scene** view, or anywhere in your computer. The supported image formats are .png and .jpg.

To use an image in the **Generations** panel:

- Right-click the image and select **Set as Reference**.
- Drag the image directly to the **Input Image** panel.

To import other images:

- In the **Input Image** panel, select **Import** and choose the image file.
- Drag the image directly to the **Input Image** panel.

## Generate based on color

You can generate textures with a matching color of the reference image.

1. In the **Input Image** panel, import a reference image.
1. Select **Color**.
1. To control how closely the generated textures follow the color of the referenced image, move the **Strength** slider. Higher values result in closer color matching.
1. In the **Prompt** field, write a prompt that describes the texture you want to generate.
1. In the **Negative Prompt** field, write a prompt that describes the elements to exclude from the generated textures.
1. To set the number of images to generate, move the **Images** slider.
1. Select **Generate**. The generated textures appear in the **Generations** panel.

## Generate based on shape

You can generate textures that resemble the shape of the reference image.

1. In the **Input Image** panel, import a reference image.
1. Select **Shape**.
1. To control how closely the generated textures follow the shape of the referenced image, move the **Tightness** slider. Higher values result in closer shape resemblance.
1. In the **Prompt** field, write a prompt that describes the texture you want to generate.
1. In the **Negative Prompt** field, write a prompt that describes the elements to exclude from the generated textures.
1. To set the number of images to generate, move the **Images** slider.
1. Select **Generate**. The generated textures appear in the **Generations** panel.

## Generate based on pattern

A pattern is an image that contains a repeatable design in black and white. You can generate a texture with a cohesive look based on the white areas of the pattern image.

1. In the **Input Image** panel, select **Shape**.
1. Select **Patterns**. A list of patterns appears.
1. Select a pattern from the list. The pattern appears in the **Input Image** panel.
1. To set how closely the generated textures follow the pattern, move the **Tightness** slider. Higher values result in closer pattern cohesion.
1. In the **Prompt** field, write a prompt that describes the texture you want to generate.
1. In the **Negative Prompt** field, write a prompt that describes the elements to exclude from the generated textures.
1. To set the number of images to generate, move the **Images** slider.
1. Select **Generate**. The generated textures appear in the **Generations** panel.

## Additional resources

* [Refine with masking](xref:refine-with-masking)
* [Upscale the generated texture](xref:upscale)
* [Create variations of the generated texture](xref:create-variations)
* [Generate textures](xref:generate)
