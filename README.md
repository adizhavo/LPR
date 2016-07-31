# LPR

[![License plate recognition](http://img.youtube.com/vi/YnLU98IplNM/0.jpg)](https://youtu.be/YnLU98IplNM)

This application is capable of localizing and recognizing license plates of European cars.
It goes through several steps from machine learning, image processing, code acquisition and OCR .

The learning session is done before building the Software, saves simple patterns of the alphabet letters in different fonts and later used at the final step of the process by the OCR to output the recognized string.

The client executes asyncroniously the following order of steps to extract pattern of letters and numbers from the image:
- Image gray-scale.
- Contrast enhancement.
- Gamma correction.
- Gaussian Filter.
- Image binarization.
- Edges and close contours recognition.
- Contours filter.
- Pattern filter.
- Slant and skew Correction.
- OCR.
