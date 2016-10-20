FROM mono:onbuild
 
EXPOSE 8080
 
CMD ["mono", "./akka-cluster-router.exe"]
