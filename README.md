# A c# console application for final project of COMPX556_22B
----
Both serial and parallel version of ACO(Ant colony optimiztion) have been implemented for MMDP (max-min diversity probelm)

## File structure
### Execution file
`/MMDP/bin/Debug/netcoreapp3.1/MMDP.exe`  

### Data set and test result
`/datasetAndResult/`

## Command line options
### Generate Option
Generate ditance matrices as test instances  
command line: `MMDP gen -o 100 2 "D:\Temp\instances.csv"`  
Generate 2 distance matrices with n equal to 100, then write to file.

### Serial ACO Option
- command line:  `MMDP sSolve "D:\Temp\instances.csv" 0.3 1000 20`  
Solve first matrix with m equal to 0.3n, max iteration 1000, 20 ants.  
MMDP sSolve [data set file] [m] [n] [number of ants]  

- command line:  `MMDP sSolve "D:\Temp\instances.csv" 0.3 1000 20 -n 10`  
Solve first matrix 10 times with m equal to 0.3n, max iteration 1000, 20 ants.  
MMDP sSolve [data set file] [m] [n] [number of ants] -n [number of executions]  

- command line:  `MMDP sSolve "D:\Temp\instances.csv" 0.3 1000 20 -a -n 10`  
Solve matrices 10 times with m equal to 0.3n, max iteration 1000, 20 ants.  
MMDP sSolve [data set file] [m] [n] [number of ants] -allMatrices -n [number of executions]  

- command line:  `MMDP sSolve "D:\Temp\instances.csv" 0.3 1000 20 -a -n 10 -w "D:\Temp\result.txt"`  
Solve matrices 10 times with m equal to 0.3n, max iteration 1000, 20 ants. Write result to file.  
MMDP sSolve [data set file] [m] [n] [number of ants] -allMatrices -n [number of executions] -writeTo filepath

### Parallel ACO Option
- command line:  `MMDP pSolve "D:\Temp\instances.csv" 0.3 1000 10 2`  
Solve first matrix with m equal to 0.3n, max iteration 1000, 2 colonies, each 10 ants.  
MMDP pSolve [data set file] [m] [n] [number of ants] [number of colonies]

- command line:  `MMDP pSolve "D:\Temp\instances.csv" 0.3 1000 10 2 -n 10`  
Solve first matrix 10 times with m equal to 0.3n, max iteration 1000, 2 colonies, each 10 ants.  
MMDP sSolve [data set file] [m] [n] [number of ants] -n [number of executions] [number of colonies] -n [number of executions]  

- command line:  `MMDP pSolve "D:\Temp\instances.csv" 0.3 1000 10 2 -a -n 10`  
Solve matrices 10 times with m equal to 0.3n, max iteration 1000, 2 colonies, each 10 ants.
MMDP sSolve [data set file] [m] [n] [number of ants] [number of colonies] -allMatrices -n [number of executions]  

- command line:  `MMDP pSolve "D:\Temp\instances.csv" 0.3 1000 10 2 -a -n 10 -w "D:\Temp\result.txt"`  
Solve matrices 10 times with m equal to 0.3n, max iteration 1000, 20 ants. Write result to file.
MMDP sSolve [data set file] [m] [n] [number of ants] [number of colonies] -allMatrices -n [number of executions] -writeTo filepath

