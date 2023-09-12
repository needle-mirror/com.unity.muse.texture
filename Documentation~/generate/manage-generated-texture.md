---
uid: manage-textures
---

# Manage generated textures

You can manage the generated images in the **Generations** panel.

## Star

To mark an image as a favorite, select the **Star** icon. To remove the star, select the **Star** icon again.

## Filter

To filter the generated images, enter a keyword in the **Search** box. The keyword can be a full or partial match to the prompt you used to generate the textures.

To show only starred images, select the **Star** icon next to the **Search** box.

## Scale

To make the generated images bigger or smaller, move the slider at the bottom of the **Generations** panel.

## Save

A texture is saved as a .png file in your project. You can use it in your project like any other texture.

A [PBR material](xref:view-as-pbr) is saved as a .mat file with a matching folder containing the PBR maps in your project. You can use it in your project like any other [material](https://docs.unity3d.com/Manual/Materials.html).

To save one or more generated images:

1. Select the generated images you want to save. To select multiple images, hold <kbd>Ctrl</kbd> (macOS: <kbd>Cmd</kbd>) and click the images.
1. Do one of the following:

    * Drag the images to the `Assets` folder in the Project window.
    * Right-click a selected image and select **Save Image** or **Save PBR**.

> [!TIP]
> To keep all your generated images, close the **Muse Generator** window and select **Save**. This saves your **Muse Generator** thread with all images in it as a generator asset in your project.

## Delete

To delete one or more generated images:

1. Select the generated image you want to delete. To select multiple images, hold <kbd>Ctrl</kbd> (macOS: <kbd>Cmd</kbd>) and click the images.
1. Right-click the image and select **Delete** (or press <kbd>Delete</kbd>).

> [!TIP]
> To delete all generated images, close the **Muse Generator** window and select **Discard**.

## Additional resources

* [Write a prompt](xref:write-prompt)
* [Refine generated textures](xref:refine)
* [Generate textures](xref:generate)