#!/usr/bin/env bash

# Halt on errors
set -e

# Specifies locale for all users
echo "LANGUAGE=en_US.UTF-8
LC_ALL=en_US.UTF-8
LANG=en_US.UTF-8
LC_TYPE=en_US.UTF-8
" > /etc/environment

# Install python
apt-get install -y python-pip
apt-get install -y python-dev # needed for speed ups to Flask

# Install app's python dependencies
pip install Flask

# Set working dir to /app on login
grep "cd /app" .bashrc > /dev/null || echo "cd /app" >> .bashrc
