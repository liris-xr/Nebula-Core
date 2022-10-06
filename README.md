# Nebula: an affordable open-source and autonomous olfactory display for VR headsets (VRST 2022)
 
This repository is the official implementation of “Nebula: an affordable open-source and autonomous olfactory display for VR headsets”.

## Description 

The impact of olfactory cues on user experience in virtual reality is increasingly studied. However, results are still heterogeneous and existing studies difficult to replicate, mainly due to a lack of standardized olfactory displays. In that context, we present Nebula, a **_low-cost_** (less than 100€), **_open-source_**, olfactory display capable of diffusing scents at *different diffusion rates* using a nebulization process. Nebula can be used with **PC VR** or **autonomous** head mounted displays, making it easily transportable without the need for an external computer.

Nebula can be easily replicated using a 3D printers and some basic electronic skills. On this repo, you will find anything needed in order to build our proposed olfactory display. 

### How does it work?

Nebula create an odorant mist using an **ultrasonic atomizer**, powered by a *pulse width modulated signal* (PWM). As a result, increasing the duty cycle results in an increased rate of diffusion.
This modulation, handled by a micro-controller (we used an Arduino Nano Every) allows Nebula to reach several olfactory intensities.

In addition to the atomizer, we used two fans which are useful to control our air flow. The smallest one, *5VDC 25x25x6mm*, is always active and forces a constant air flow to be able to smell the odor more rapidly after its diffusion and mitigates the pressure drop when enabling the extraction fan. This one is a *12VDC 40x40x20mm*, much stronger, that runs for 2 seconds in order to extract the remaining odorant and thus improve the reactivity of the device.
