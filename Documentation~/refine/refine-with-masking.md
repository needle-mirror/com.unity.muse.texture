---
uid: refine-with-masking
---

# Refine with masking

Masking provides enhanced control over the output.

## Refine a texture

Use a mask to refine any part of a generated texture. Adjust the mask with brush settings and an eraser.

1. In the **Generations** panel, hover over the texture and select the **Refine** icon to enter the Refinements branch.

   > [!TIP]
   > You can also double-click a generated texture to enter the Refinements branch.

1. To activate the masking brush, select the **Inpaint** icon.
1. To adjust the size of the brush, move the **Radius** slide.
1. Paint a mask over the area that you want to refine.

    ![Example masking](../images/masking.png)
1. To erase the mask, select **Eraser**. The eraser uses the same radius as the brush.
1. Enter a prompt that describes the desired refinement.
1. Select **Generate**. This regenerates textures in the masked area.

## Set as thumbnail

To set a refined texture as the thumbnail in the **Generations** panel for the Refinements branch, in the **Refinements** panel, right-click the texture and select **Set as Thumbnail**.

To re-enter the Refinements branch from the  **Generations** panel, double-click the thumbnail.

## Create a new refinement branch

To create a new refinement branch from a refined texture, in the **Refinements** panel, right-click the texture and select **Branch**. This adds the selected texture to the **Generations** panel and creates a new refinement branch with the selected texture as the root.

## Additional resources

* [Set as a reference image](xref:set-as-reference)
* [Upscale the generated texture](xref:upscale)
* [Create variations of the generated texture](xref:create-variations)
* [Generate textures](xref:generate)