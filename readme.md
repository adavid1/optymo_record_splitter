# Optymo Record Splitter

This application allows you to split daily radio recordings into sorted and timestamped conversations.

## Project Development
Developed as a project for Crunch Time 2019 in collaboration with UTBM.

## Tutorial

The input data for the application includes:
- **Audio File**: The radio recording in `.mp3` format.
- **Start Date and Time**: The date and time when the recording started, used to timestamp the individual conversations.
- **Minimum Split Tolerance (in minutes)**: The minimum desired time between two conversations.
  - Recommended value: 60 minutes
- **Ignored Volume Tolerance (in %)**: The volume level below which the audio is considered noise and ignored.
  - Recommended value: 30% (adjust according to background noise)

Once the input data is entered and confirmed, the application creates a folder in the same location as the source audio file, named after the recording date. This folder will contain all the split and timestamped conversations.

## How to Use
1. Launch the application.
2. Input the required data:
   - Select the `.mp3` audio file.
   - Enter the start date and time of the recording.
   - Set the minimum split tolerance in minutes.
   - Set the ignored volume tolerance in percentage.
3. Click on the confirm button to start the process.
4. The application will create a folder named after the recording date in the same directory as the audio file. This folder will contain all the split conversations, each one timestamped.

## Acknowledgements

- UTBM for the collaboration and support during the development of this project.
