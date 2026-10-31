import bpy, math, random, os
from mathutils import Vector
base=r'N:\unity\projects\EcoClean'
kit=bpy.data.scenes.get('EcoClean Park Kit')
scene=bpy.data.scenes.new('EcoClean Park Environment');bpy.context.window.scene=scene
random.seed(31)
def prop(name,x,y,yaw=0,scale=1):
 src=kit.objects[name];o=src.copy();o.data=src.data;o.name=name;scene.collection.objects.link(o);o.location=(x,-y,0);o.rotation_euler=(0,0,-math.radians(yaw));o.scale=(scale,scale,scale);o.hide_set(False);return o
def material(name,c):
 m=bpy.data.materials.get(name) or bpy.data.materials.new(name);m.diffuse_color=(*c,1);return m
grass=material('Park grass',(.36,.51,.16));sand=material('Park paths',(.76,.64,.43))
def box(name,x,y,z,s,mat):
 bpy.ops.mesh.primitive_cube_add(size=1,location=(x,-y,z));o=bpy.context.object;o.name=name;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mat);return o
def ring(name,inner,outer,z,mat):
 n=96;v=[];f=[]
 for i in range(n):
  a=i*math.tau/n;v.extend([(inner*math.cos(a),inner*math.sin(a),z),(outer*math.cos(a),outer*math.sin(a),z)])
 for i in range(n):j=(i+1)%n;f.append((i*2,i*2+1,j*2+1,j*2))
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],f);mesh.update();o=bpy.data.objects.new(name,mesh);scene.collection.objects.link(o);mesh.materials.append(mat)
box('Ground',0,0,-.2,(50,50,.4),grass)
ring('Loop path',11.5,14.5,.03,sand);ring('Fountain plaza',0,6,.04,sand)
for x,y,s in [(0,-15,(4,20,.05)),(0,15,(4,20,.05)),(-9,0,(10,3,.05)),(9,0,(10,3,.05)),(-15,13,(10,7,.05)),(15,13,(8,6,.05))]:box('Path',x,y,.025,s,sand)
prop('Fountain',0,0)
for i in range(4):
 a=math.radians(45+i*90);prop('Bench',math.sin(a)*7.7,math.cos(a)*7.7,45+i*90)
prop('PicnicTable',-16,14,-8);prop('PicnicTable',-13,11,12)
for i,name in enumerate(['BinPaper','BinPlastic','BinMetal']):prop(name,13+i*1.7,14)
prop('Sign',3.5,-22);prop('PlantingBed',15,-12)
for i in range(-11,12):
 if abs(i)>1:prop('Fence',i*2,-24);prop('Fence',i*2,24)
 prop('Fence',-24,i*2,90);prop('Fence',24,i*2,90)
for i in range(52):
 edge=i%4;along=random.uniform(-22,22)
 if edge==0:x,y=-21+random.uniform(-1,1),along
 elif edge==1:x,y=21+random.uniform(-1,1),along
 elif edge==2:x,y=along,-21+random.uniform(-1,1)
 else:x,y=along,21+random.uniform(-1,1)
 if abs(x)<4 and abs(y)>19:continue
 prop(['TreeOak','TreeLime','TreeDark'][i%3],x,y,random.uniform(0,360),random.uniform(.9,1.45))
for i in range(110):
 a=random.uniform(0,math.tau);r=random.uniform(15.3,22);x,y=math.cos(a)*r,math.sin(a)*r
 if abs(x)<3 or (8<y<18 and 9<abs(x)<20):continue
 prop('Bush' if i%2==0 else 'BushLight',x,y,random.uniform(0,360),random.uniform(.65,1.15))
 if i%4==0:prop('Rock',x+.8,y+.5,random.uniform(0,360),random.uniform(.5,1))
 if i%2==0:prop('Flower',x+.7,y-.3,0,random.uniform(.7,1.3))
for i in range(15):prop(['LitterBottle','LitterCup','LitterPaper'][i%3],random.uniform(-19,-10),random.uniform(9,17),random.uniform(0,360),1.3)
world=bpy.data.worlds.new('Park World');scene.world=world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs['Color'].default_value=(.65,.77,.91,1);world.node_tree.nodes['Background'].inputs['Strength'].default_value=.5
data=bpy.data.lights.new('Afternoon Sun','SUN');data.energy=2.5;data.angle=.12;sun=bpy.data.objects.new('Afternoon Sun',data);scene.collection.objects.link(sun);sun.rotation_euler=(.65,-.35,-.7)
data=bpy.data.cameras.new('Overview');camera=bpy.data.objects.new('Overview',data);scene.collection.objects.link(camera);camera.location=(32,42,38);camera.rotation_euler=(Vector((0,0,0))-camera.location).to_track_quat('-Z','Y').to_euler();data.lens=40;scene.camera=camera
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=1600;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
for area in bpy.context.screen.areas:
 if area.type=='CONSOLE':area.type='VIEW_3D';area.spaces.active.shading.color_type='MATERIAL';area.spaces.active.region_3d.view_distance=65;area.spaces.active.region_3d.view_location=(0,0,0)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(base,'SourceArt','Park','EcoClean_Park_Environment.blend'))
