clear all;
close all;
clc;
fileData = csvread("E:\StartInnovating\Projects\1_Neurosky_robot_control_EEG\Matlab_Code\finalData.csv");
%%
forward = [];
backward = [];
right = [];
left = [];
stop = [];
%size(object,1-row/2-column)
disp("dividing data into different object based on class.")
for i=1:size(fileData,1)
    if fileData(i,2) == 0
        forward = [forward fileData(i,1)];
    elseif fileData(i,2) == 1
        backward = [backward fileData(i,1)];
    elseif fileData(i,2) == 2
        left = [left fileData(i,1)];
    elseif fileData(i,2) == 3
        right = [right fileData(i,1)];
    elseif fileData(i,2) == 4
        stop = [stop fileData(i,1)];
    end
    disp(i)
end
%%
csvwrite("E:\StartInnovating\Projects\1_Neurosky_robot_control_EEG\Matlab_Code\" + "forward.csv",forward);
csvwrite("E:\StartInnovating\Projects\1_Neurosky_robot_control_EEG\Matlab_Code\" + "backward.csv",backward);
csvwrite("E:\StartInnovating\Projects\1_Neurosky_robot_control_EEG\Matlab_Code\" + "left.csv",left);
csvwrite("E:\StartInnovating\Projects\1_Neurosky_robot_control_EEG\Matlab_Code\" + "right.csv",right);
csvwrite("E:\StartInnovating\Projects\1_Neurosky_robot_control_EEG\Matlab_Code\" + "stop.csv",stop);


disp("Completed dividing data.")

%%
%to calculate the feature_matrix
global feature_matrix;
feature_matrix = [];

%%
%save data to csv file
csvwrite("E:\StartInnovating\Projects\1_Neurosky_robot_control_EEG\Matlab_Code\" + "data.csv",feature_matrix);
%labels as follows
%1 - rawValue
%2 - amp_IMF1
%3 - amp_IMF2
%4 - amp_IMF3
%5 - amp_IMF4
%6 - freq_IMF1
%7 - freq_IMF2
%8 - freq_IMF3
%9 - freq_IMF4
%10 - class
%%
pause on;
disp("Starting feature extraction...")
pause (0.05)
feature_matrix = [feature_matrix; feature_extraction(forward,0)];
disp("completed forward class feature extraction.")
pause (0.05)
feature_matrix = [feature_matrix; feature_extraction(backward, 1)];
disp("completed backward class feature extraction.")
pause (0.05)
feature_matrix = [feature_matrix; feature_extraction(left,2)];
disp("completed left class feature extraction.")
pause (0.05)
feature_matrix = [feature_matrix; feature_extraction(right, 3)];
disp("completed right class feature extraction.")
pause (0.05)
feature_matrix = [feature_matrix; feature_extraction(stop, 4)];
disp("completed stop class feature extraction.")

disp("completed feature extraction.")
function feature_matrix = feature_extraction(class, value)
k=1;
Fs=10;            %Sampling frequency
feature_matrix = [];
while 1
   %performing Empirical Mode Decomposition
   try
    [imf, residual] = emd(class(1,k:k+511));
   catch err
       disp(err)
       break
   end
   % performing the HILBERT TRANSFORM
   hx = hilbert(imf);
   % calculating the INSTANTANEOUS AMPLITUDE (ENVELOPE)
   inst_amp = abs(hx);
   % calculating the INSTANTANEOUS FREQUENCY
   inst_freq = diff(unwrap(angle(hx)))/((1/Fs)*2*pi);
   %writing to feature matrix
   m=1; % value for amplitude and frequency matrix
   for i=k:k+511
           try
           matrix = [class(1,i) transpose(inst_amp(1:4,m)) transpose(inst_freq(1:4,m))];
           catch err
               disp(err)
           end
           %if the size of matrix is not 9 then duplicate a random column
           while(size(matrix,2) ~= 9)
                matrix = [matrix matrix(1,randi([1,6]))];
           end
           feature_matrix =[feature_matrix; matrix value];
           m = m + 1;
   end
   disp("iteration of class " + value + " : " + k)
   pause (0.05)
   k = k + 512;   
end
end