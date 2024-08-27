#!/usr/bin/env bash

#* Install full Gurobi
cd /opt
wget https://packages.gurobi.com/11.0/gurobi_server11.0.3_linux64.tar.gz
tar xvfz gurobi_server11.0.3_linux64.tar.gz 
mv gurobi_server1103/ gurobi1103/
cd ~
#* Set up Gurobi environment variables
echo "# Gurobi" >> ~/.bashrc
echo "export GRB_LICENSE_FILE=/users/hy/gurobi.lic" >> ~/.bashrc
echo "export GUROBI_HOME=\"/opt/gurobi1103/linux64\"" >> ~/.bashrc
echo "export PATH=\"\${PATH}:\${GUROBI_HOME}/bin\"" >> ~/.bashrc
echo "export LD_LIBRARY_PATH=\"\${GUROBI_HOME}/lib\"" >> ~/.bashrc
echo "export LD_LIBRARY_PATH=\"\${LD_LIBRARY_PATH}:\${GUROBI_HOME}/lib\"" >> ~/.bashrc
#* Get mopt
git clone https://github.com/HongyuHe/mopt.git
cd mopt
git checkout hongyu/setup
#* Retrieve Gurobi license
grbgetkey ce55f8fd-d60a-4be9-be5c-521d9e860a86

