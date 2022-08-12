# 3D-TMO

Unity project of 3D-TMO: [Tone mapping High Dynamic 3D scenes with global lightness coherency](https://igoude.github.io/).

> "Tone mapping High Dynamic 3D scenes with global lightness coherency; I. Goudé, J. Lacoche, R. Cozot; Computer & Graphics (2020); https://doi.org/10.1016/j.cag.2020.08.004"

## Requirements

Unity 2019.2.4f1  
SteamVR (up to date)  
This TMO has been developed using an HTC Vive pro HMD

## Abstract

We propose a new approach for real-time Tone Mapping Operator dedicated to High Dynamic Range rendering of interactive 3D scenes. <br />
The proposed method considers the whole scene lighting in order to preserve the global coherency.

<html>
    <body>
        <p align="center">
            <img src="Docs/images/scene.png" alt="scene" height="200" >
        </p>
    </body>
</html>   

The 3D scene consists of two rooms, one very bright and one very dark, separated by a door.

 <br />

<html>
    <body>
        <p align="center">
            <img src="Docs/images/combine1_bright.png" alt="TMO Framework" height="100">
            <img src="Docs/images/combine2_bright.png" alt="TMO Framework" height="100"> 
            <img src="Docs/images/combine3_bright.png" alt="TMO Framework" height="100"> <br />
            <img src="Docs/images/combine1_dark.png" alt="TMO Framework" height="100">
            <img src="Docs/images/combine2_dark.png" alt="TMO Framework" height="100">
            <img src="Docs/images/combine3_dark.png" alt="TMO Framework" height="100">
        </p>
    </body>
</html>

Comparison between Viewport TMO (left), Global TMO (right) and the combination of both TMOs (middle) for two different viewpoints in the scene.

## Contact

> Ific Goudé <br />
Research Scientist in Computer Graphics <br />
Email: goude.ific@gmail.com <br />
Website: [https://igoude.github.io/](https://igoude.github.io/)
