# Nebula: an affordable open-source and autonomous olfactory display for VR headsets (VRST 2022)
 
This repository is the official implementation of “Nebula: an affordable open-source and autonomous olfactory display for VR headsets”.

## Description 

The impact of olfactory cues on user experience in virtual reality is increasingly studied. However, results are still heterogeneous and existing studies difficult to replicate, mainly due to a lack of standardized olfactory displays. In that context, we present Nebula, a **_low-cost_** (less than 100€), **_open-source_**, olfactory display capable of diffusing scents at *different diffusion rates* using a nebulization process. Nebula can be used with **PC VR** or **autonomous** head mounted displays, making it easily transportable without the need for an external computer.

Nebula can be easily replicated using a 3D printers and some basic electronic skills. On this repo, you will find anything needed in order to build our proposed olfactory display. 

### How does it work?

Nebula create an odorant mist using an **ultrasonic atomizer**, powered by a *pulse width modulated signal* (PWM). As a result, increasing the duty cycle results in an increased rate of diffusion.
This modulation, handled by a micro-controller (we used an Arduino Nano Every) allows Nebula to reach several olfactory intensities.

In addition to the atomizer, we used two fans which are useful to control our air flow. The smallest one, *5VDC 25x25x6mm*, is always active and forces a constant air flow to be able to smell the odor more rapidly after its diffusion and mitigates the pressure drop when enabling the extraction fan. This one is a *12VDC 40x40x20mm*, much stronger, that runs for 2 seconds in order to extract the remaining odorant and thus improve the reactivity of the device.

Nebula is ready to use with Unity (PC VR built or in editor), using the provided Unity-Package or scripsts from the Nebula-UnitySoftware folder.
Furthemore, Nebula works standalone using an Android Archive Library using the provided package on your own built scene. (Tested on a Meta Quest 2)

## Repository Structure

  * [Nebula-CAD](https://github.com/Plateforme-VR-ENISE/Nebula-Core/tree/master/Nebula-CAD) contains all STL files (ready to print), a PDF version of the wiring diagram, a list of the necessary components and some printings recommendations.
  * [Nebula-Experiment](https://github.com/Plateforme-VR-ENISE/Nebula-Core/tree/master/Nebula-Experiment) contains results and the unity project used in our experiment described in our paper.
  * [Nebula-Firmware](https://github.com/Plateforme-VR-ENISE/Nebula-Core/tree/master/Nebula-Firmware) contains code for the microcontroller used (Arduino Nano Every).
  * [Nebula-UnitySoftware](https://github.com/Plateforme-VR-ENISE/Nebula-Core/tree/master/Nebula-UnitySoftware) is the Unity project using Nebula in VR including a sample scene with an orange ready to diffuse when approached to the nose !
  
   ## Release content
   
   * ``Nebula-UnityPackage`` Unity Package which includes everything needed to bring Nebula into your project unity.
   * ``Nebula-PCSampleScene`` sample scene included in the repository built that countains a ready-to-use application for Nebula (perfect to test yours !)
   * ``Nebula-SampleScene`` same as above but built for Meta Quest 2.
